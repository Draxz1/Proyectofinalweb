using megadeliciasapi.Data;
using megadeliciasapi.Models;
using megadeliciasapi.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace megadeliciasapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "admin,cocinero")]
    public class CocinaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CocinaController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetTableroCocina()
        {
            var estadosActivos = new[] { "PENDIENTE", "EN_PROCESO", "LISTO", "ENTREGADO", "CANCELADO" };

            // Solo mostramos órdenes del día actual para no saturar la vista
            var hoy = DateTime.Today;
            var mañana = hoy.AddDays(1);

            var ordenes = await _context.Ordenes
                .Include(o => o.Estado) 
                .Include(o => o.Detalles)
                    .ThenInclude(d => d.Plato)
                .Include(o => o.Usuario)
                .Where(o => estadosActivos.Contains(o.Estado.Nombre) 
                         && o.Fecha >= hoy 
                         && o.Fecha < mañana)
                .OrderBy(o => o.Fecha) // FIFO (Lo más viejo primero)
                .Select(o => new
                {
                    o.Id,
                    o.Fecha,
                    Estado = o.Estado.Nombre,
                    Mesero = o.Usuario != null ? o.Usuario.Nombre : "Mesero " + o.UsuarioId,
                    Platos = string.Join(", ", o.Detalles.Select(d => $"{d.Cantidad}x {d.Plato.Nombre}")),
                    Items = o.Detalles.Select(d => new { 
                        d.Cantidad, 
                        Nombre = d.Plato.Nombre, 
                        Nota = d.NotaPlato 
                    })
                })
                .ToListAsync();

            return Ok(ordenes);
        }
        [HttpGet("historial")]
        public async Task<IActionResult> GetHistorial([FromQuery] int dias = 7)
        {
            var fechaInicio = DateTime.Today.AddDays(-dias);

            var ordenes = await _context.Ordenes
                .Include(o => o.Estado) 
                .Include(o => o.Detalles)
                    .ThenInclude(d => d.Plato)
                .Include(o => o.Usuario)
                .Where(o => o.Fecha >= fechaInicio)
                .OrderByDescending(o => o.Fecha)
                .Select(o => new
                {
                    o.Id,
                    o.Fecha,
                    Estado = o.Estado.Nombre,
                    Mesero = o.Usuario != null ? o.Usuario.Nombre : "Mesero " + o.UsuarioId,
                    Items = o.Detalles.Select(d => new { 
                        d.Cantidad, 
                        Nombre = d.Plato.Nombre, 
                        Nota = d.NotaPlato 
                    })
                })
                .ToListAsync();

            return Ok(ordenes);
        }

        [HttpPut("{id}/estado")]
        public async Task<IActionResult> ActualizarEstado(int id, [FromBody] ChangeStateDto dto)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var orden = await _context.Ordenes
                    .Include(o => o.Estado)
                    .Include(o => o.Detalles)
                        .ThenInclude(d => d.Plato)
                    .FirstOrDefaultAsync(o => o.Id == id);
                    
                if (orden == null) 
                    return NotFound(new { message = "Orden no encontrada" });

                var nuevoEstado = await _context.EstadosOrden
                    .FirstOrDefaultAsync(e => e.Nombre == dto.Estado);
                
                if (nuevoEstado == null) 
                    return BadRequest(new { message = $"Estado '{dto.Estado}' no válido" });

                // Validar transiciones de estado
                if (!EsTransicionValida(orden.Estado.Nombre, dto.Estado))
                {
                    return BadRequest(new { 
                        message = $"No se puede cambiar de {orden.Estado.Nombre} a {dto.Estado}" 
                    });
                }

                // 🔥 LÓGICA DE INVENTARIO: Si pasa a EN_PROCESO, descontar inventario
                if (dto.Estado == "EN_PROCESO" && orden.Estado.Nombre == "PENDIENTE")
                {
                    var resultadoInventario = await DescontarInventario(orden);
                    if (!resultadoInventario.Exitoso)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest(new { 
                            message = "No hay suficiente inventario para procesar esta orden",
                            detalles = resultadoInventario.Mensaje
                        });
                    }
                }

                // Actualizar estado
                orden.EstadoId = nuevoEstado.Id;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { 
                    message = $"Orden #{id} movida a {dto.Estado}",
                    nuevoEstado = dto.Estado
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { 
                    message = "Error al actualizar el estado de la orden",
                    error = ex.Message 
                });
            }
        }

        private bool EsTransicionValida(string estadoActual, string nuevoEstado)
        {
            var transicionesPermitidas = new Dictionary<string, string[]>
            {
                { "PENDIENTE", new[] { "EN_PROCESO", "CANCELADO" } },
                { "EN_PROCESO", new[] { "LISTO", "CANCELADO" } },
                { "LISTO", new[] { "ENTREGADO" } },
                { "ENTREGADO", new string[] { } }, // Estado final
                { "CANCELADO", new string[] { } }  // Estado final
            };

            return transicionesPermitidas.ContainsKey(estadoActual) 
                && transicionesPermitidas[estadoActual].Contains(nuevoEstado);
        }

        [HttpPost("{id}/notificar")]
        public async Task<IActionResult> NotificarMesero(int id)
        {
            var orden = await _context.Ordenes
                .Include(o => o.Usuario)
                .Include(o => o.Estado)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (orden == null)
                return NotFound(new { message = "Orden no encontrada" });

            if (orden.Estado.Nombre != "LISTO")
                return BadRequest(new { message = "Solo se pueden notificar órdenes listas" });
            
            Console.WriteLine($"[NOTIFICACIÓN] Orden #{id} lista para {orden.Usuario?.Nombre ?? "Mesero"}");

            return Ok(new { 
                message = $"Notificación enviada a {orden.Usuario?.Nombre ?? "mesero"}",
                ordenId = id
            });
        }

        private async Task<(bool Exitoso, string Mensaje)> DescontarInventario(Orden orden)
        {
            var ingredientesFaltantes = new List<string>();

            foreach (var detalle in orden.Detalles)
            {

                var ingredientes = await _context.PlatoIngredientes
                    .Where(pi => pi.PlatoId == detalle.PlatoId)
                    .Include(pi => pi.InventarioItem)
                    .ToListAsync();

                if (!ingredientes.Any())
                {
                    ingredientesFaltantes.Add($"❌ {detalle.Plato.Nombre} (sin receta configurada)");
                    continue;
                }

                foreach (var ingrediente in ingredientes)
                {
                    if (ingrediente.InventarioItem == null)
                    {
                        ingredientesFaltantes.Add($"❌ {detalle.Plato.Nombre} (ingrediente no vinculado en inventario)");
                        continue;
                    }

                    decimal cantidadNecesaria = ingrediente.CantidadUsada * detalle.Cantidad;
                    
                    if (ingrediente.InventarioItem.StockActual < cantidadNecesaria)
                    {
                        ingredientesFaltantes.Add(
                            $"❌ {ingrediente.InventarioItem.Nombre}: Necesitas {cantidadNecesaria} {ingrediente.UnidadMedida}, solo hay {ingrediente.InventarioItem.StockActual}"
                        );
                        continue;
                    }

                    ingrediente.InventarioItem.StockActual -= (int)Math.Ceiling(cantidadNecesaria);

                    _context.InventarioMovimientos.Add(new InventarioMovimiento
                    {
                        ItemId = ingrediente.ItemId,
                        Tipo = "SALIDA",
                        Cantidad = (int)Math.Ceiling(cantidadNecesaria),
                        CostoUnitario = ingrediente.InventarioItem.CostoUnitario,
                        Motivo = $"Orden #{orden.Id} - {detalle.Plato.Nombre} (x{detalle.Cantidad})",
                        Fecha = DateTime.Now
                    });
                }
            }

            if (ingredientesFaltantes.Any())
            {
                return (false, string.Join(" | ", ingredientesFaltantes));
            }

            return (true, "Inventario descontado correctamente");
        }
    }
}