using megadeliciasapi.Data;
using megadeliciasapi.Models;
using megadeliciasapi.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace megadeliciasapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
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
            try 
            {
                var estadosActivos = new[] { "PENDIENTE", "EN_PROCESO", "LISTO", "ENTREGADO", "CANCELADO" };

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
                    .OrderBy(o => o.Fecha)
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
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener el tablero", error = ex.Message });
            }
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

                string estadoSolicitado = dto.Estado.ToUpper().Trim();

                var nuevoEstado = await _context.EstadosOrden
                    .FirstOrDefaultAsync(e => e.Nombre == estadoSolicitado);
                
                if (nuevoEstado == null) 
                    return BadRequest(new { message = $"Estado '{estadoSolicitado}' no es válido en el sistema." });

                if (!EsTransicionValida(orden.Estado.Nombre, estadoSolicitado))
                {
                    return BadRequest(new { 
                        message = $"No se puede cambiar de {orden.Estado.Nombre} a {estadoSolicitado}" 
                    });
                }

                if (estadoSolicitado == "EN_PROCESO" && orden.Estado.Nombre == "PENDIENTE")
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

                orden.EstadoId = nuevoEstado.Id;
                await _context.SaveChangesAsync();
                
                await transaction.CommitAsync();

                return Ok(new { 
                    message = $"Orden #{id} movida a {estadoSolicitado}",
                    nuevoEstado = estadoSolicitado
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { 
                    message = "Error crítico al actualizar el estado de la orden",
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
                { "LISTO", new[] { "ENTREGADO", "EN_PROCESO" } },
                { "ENTREGADO", new string[] { } },
                { "CANCELADO", new string[] { } }
            };

            if (!transicionesPermitidas.ContainsKey(estadoActual)) return false;

            return transicionesPermitidas[estadoActual].Contains(nuevoEstado);
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
                return BadRequest(new { message = "Solo se pueden notificar órdenes que estén LISTAS" });
            
            Console.WriteLine($"[NOTIFICACIÓN] Orden #{id} lista para {orden.Usuario?.Nombre ?? "Mesero"}");

            return Ok(new { 
                message = $"Notificación enviada a {orden.Usuario?.Nombre ?? "el mesero"}",
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
                    continue;
                }

                foreach (var ingrediente in ingredientes)
                {
                    if (ingrediente.InventarioItem == null) 
                    {
                        ingredientesFaltantes.Add($"⚠️ Ingrediente no configurado para {detalle.Plato?.Nombre ?? "plato"}");
                        continue;
                    }

                    decimal cantidadNecesaria = ingrediente.CantidadUsada * detalle.Cantidad;
                    
                    if (ingrediente.InventarioItem.StockActual < (int)Math.Ceiling(cantidadNecesaria))
                    {
                        ingredientesFaltantes.Add(
                            $"❌ {ingrediente.InventarioItem.Nombre}: Requiere {cantidadNecesaria} {ingrediente.UnidadMedida}, Stock: {ingrediente.InventarioItem.StockActual}"
                        );
                        continue;
                    }

                    // ⭐ DESCONTAR CON CAST A INT (StockActual es int)
                    ingrediente.InventarioItem.StockActual -= (int)Math.Ceiling(cantidadNecesaria);

                    // Registrar movimiento
                    var movimiento = new InventarioMovimiento
                    {
                        ItemId = ingrediente.ItemId,
                        PlatoIngredienteId = ingrediente.Id,
                        Tipo = "CONSUMO_COCINA",
                        Cantidad = (int)Math.Ceiling(cantidadNecesaria),
                        CostoUnitario = ingrediente.InventarioItem.CostoUnitario,
                        Motivo = $"Orden #{orden.Id} - {detalle.Plato?.Nombre ?? "Plato"} x{detalle.Cantidad}",
                        Fecha = DateTime.Now
                    };

                    _context.InventarioMovimientos.Add(movimiento);
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