using megadeliciasapi.Data;
using megadeliciasapi.Models;
using megadeliciasapi.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt; 

namespace megadeliciasapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MeseroController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MeseroController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ GET: api/mesero/ordenes
        // DEVUELVE SOLO ÓRDENES EN ESTADO "LISTO" DEL MESERO ACTUAL
        [HttpGet("ordenes")]
        public async Task<IActionResult> ListarOrdenes()
        {
            var userIdStr = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value 
                         ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

            var esAdmin = User.IsInRole("admin");

            // ✅ FILTRO: Solo órdenes en estado "LISTO"
            var query = _context.Ordenes
                .Include(o => o.Estado)
                .Include(o => o.Detalles)
                    .ThenInclude(d => d.Plato)
                .Include(o => o.Usuario)
                .Where(o => o.Estado.Nombre == "LISTO") // ⚡ CRÍTICO: Solo órdenes listas
                .AsQueryable();

            // Si no es admin, solo ve sus propias órdenes
            if (!esAdmin)
            {
                query = query.Where(o => o.UsuarioId == userId);
            }

            var ordenes = await query
                .OrderBy(o => o.Fecha) // ⚡ Las más antiguas primero (FIFO)
                .Select(o => new
                {
                    id = o.Id,
                    fecha = o.Fecha,
                    estado = o.Estado.Nombre, // ✅ Devuelve "LISTO"
                    resumen = string.Join(", ", o.Detalles.Select(d => 
                        $"{d.Cantidad}x {(d.Plato != null ? d.Plato.Nombre : "Item eliminado")}"
                    )),
                    total = o.TotalOrden,
                    mesaId = o.MesaId
                })
                .ToListAsync();

            return Ok(ordenes);
        }

        // POST: api/mesero/ordenes
        // Crear nueva orden
        [HttpPost("ordenes")]
        public async Task<IActionResult> CrearOrden([FromBody] CrearOrdenDto dto)
        {
            try
            {
                if (dto.Detalles == null || !dto.Detalles.Any())
                    return BadRequest(new { message = "La orden debe tener al menos un plato." });

                var userIdStr = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value 
                             ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (!int.TryParse(userIdStr, out int userId) || userId <= 0)
                {
                    return Unauthorized(new { message = "Token inválido." });
                }

                var usuarioExiste = await _context.Usuarios.AnyAsync(u => u.Id == userId);
                if (!usuarioExiste) return Unauthorized(new { message = "Usuario no existe." });

                var estadoPendiente = await _context.EstadosOrden.FirstOrDefaultAsync(e => e.Nombre == "PENDIENTE");
                if (estadoPendiente == null)
                {
                    var estadoDefault = await _context.EstadosOrden.FindAsync(1);
                    if (estadoDefault == null)
                        return BadRequest(new { message = "Error crítico: No existen estados de orden." });
                    estadoPendiente = estadoDefault;
                }

                int mesaIdReal;
                if (dto.MesaId.HasValue && dto.MesaId > 0)
                {
                    mesaIdReal = dto.MesaId.Value;
                }
                else
                {
                    var primeraMesa = await _context.Mesas.FirstOrDefaultAsync();
                    if (primeraMesa == null) return BadRequest(new { message = "No hay mesas registradas." });
                    mesaIdReal = primeraMesa.Id;
                }

                decimal total = 0;
                var nuevosDetalles = new List<DetalleOrden>();

                foreach (var item in dto.Detalles)
                {
                    var plato = await _context.Platos.FindAsync(item.PlatoId);
                    if (plato == null) return BadRequest(new { message = $"El plato {item.PlatoId} no existe." });

                    decimal subtotal = plato.Precio * item.Cantidad;
                    total += subtotal;

                    nuevosDetalles.Add(new DetalleOrden
                    {
                        PlatoId = item.PlatoId,
                        Cantidad = item.Cantidad,
                        PrecioUnitario = plato.Precio,
                        Subtotal = subtotal,
                        NotaPlato = item.Nota
                    });
                }

                var orden = new megadeliciasapi.Models.Orden 
                {
                    UsuarioId = userId,
                    Fecha = DateTime.Now,
                    EstadoId = estadoPendiente.Id,
                    TipoOrden = "LOCAL",
                    MesaId = mesaIdReal,
                    TotalOrden = total,
                    Detalles = nuevosDetalles
                };

                _context.Ordenes.Add(orden);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Orden enviada a cocina", id = orden.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error en el servidor.", error = ex.Message });
            }
        }

        // ✅ POST: api/mesero/entregar/{ordenId}
        // Cambia el estado de LISTO a ENTREGADO
        [HttpPost("entregar/{ordenId}")]
        public async Task<IActionResult> MarcarComoEntregada(int ordenId)
        {
            try 
            {
                // 1. Buscar la orden
                var orden = await _context.Ordenes
                    .Include(o => o.Estado)
                    .FirstOrDefaultAsync(o => o.Id == ordenId);
                    
                if (orden == null) 
                    return NotFound(new { message = "Orden no encontrada" });

                // 2. Verificar que esté en estado LISTO
                if (orden.Estado.Nombre != "LISTO")
                    return BadRequest(new { message = "Solo se pueden entregar órdenes en estado LISTO" });

                // 3. Buscar el estado "ENTREGADO"
                var estadoEntregado = await _context.EstadosOrden
                    .FirstOrDefaultAsync(e => e.Nombre == "ENTREGADO");
                
                if (estadoEntregado == null) 
                    return BadRequest(new { message = "Error: El estado 'ENTREGADO' no existe en la base de datos." });

                // 4. Actualizar
                orden.EstadoId = estadoEntregado.Id;
                await _context.SaveChangesAsync();

                return Ok(new { 
                    message = "Orden marcada como entregada correctamente",
                    ordenId = ordenId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar la orden", error = ex.Message });
            }
        }
    }
}