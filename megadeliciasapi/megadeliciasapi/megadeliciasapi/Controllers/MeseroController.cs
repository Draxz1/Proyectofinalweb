using megadeliciasapi.Data;
using megadeliciasapi.Models;
using megadeliciasapi.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt; // Necesario para leer el Token

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

        // GET: api/mesero/ordenes
        // Lista las órdenes recientes
        [HttpGet("ordenes")]
        public async Task<IActionResult> ListarOrdenes()
        {
            // Obtener ID del usuario actual desde el token
            var userIdStr = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value 
                         ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

            var esAdmin = User.IsInRole("admin");

            var query = _context.Ordenes
                .Include(o => o.Estado)   // <--- FIX: Incluir Estado para ver el nombre
                .Include(o => o.Detalles)
                .ThenInclude(d => d.Plato)
                .AsQueryable();

            if (!esAdmin)
            {
                query = query.Where(o => o.UsuarioId == userId);
            }

            var ordenes = await query
                .OrderByDescending(o => o.Fecha)
                .Take(20)
                .Select(o => new
                {
                    o.Id,
                    o.Fecha,
                    Estado = o.Estado.Nombre, // <--- FIX: Devolver nombre (ej. "PENDIENTE") en vez de ID
                    Resumen = string.Join(", ", o.Detalles.Select(d => $"{d.Cantidad}x {d.Plato.Nombre}")),
                    Total = o.TotalOrden
                })
                .ToListAsync();

            return Ok(ordenes);
        }

        // POST: api/mesero/ordenes
        // Crear una nueva orden
        [HttpPost("ordenes")]
        public async Task<IActionResult> CrearOrden([FromBody] CrearOrdenDto dto)
        {
            if (dto.Detalles == null || !dto.Detalles.Any())
                return BadRequest(new { message = "La orden debe tener al menos un plato." });

            // Obtener usuario
            var userIdStr = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value 
                         ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int.TryParse(userIdStr, out int userId);

            // --- FIX: OBTENER ESTADO AUTOMÁTICAMENTE ---
            var estadoPendiente = await _context.EstadosOrden.FirstOrDefaultAsync(e => e.Nombre == "PENDIENTE");
            int estadoId = estadoPendiente?.Id ?? 1; // Fallback a 1 si no encuentra

            // --- FIX: OBTENER MESA AUTOMÁTICAMENTE (Evita error 500) ---
            // agrue este medodo para que el frontend manda mesa, la usamos; si no, tomamos la primera disponible
            int mesaIdReal;
            if (dto.MesaId.HasValue && dto.MesaId > 0)
            {
                mesaIdReal = dto.MesaId.Value;
            }
            else
            {
                var primeraMesa = await _context.Mesas.FirstOrDefaultAsync();
                if (primeraMesa == null) 
                    return BadRequest(new { message = "No hay mesas registradas. Ejecuta el Seed de Mesas." });
                mesaIdReal = primeraMesa.Id;
            }

            decimal total = 0;
            var nuevosDetalles = new List<DetalleOrden>();

            foreach (var item in dto.Detalles)
            {
                var plato = await _context.Platos.FindAsync(item.PlatoId);
                if (plato == null) return BadRequest(new { message = $"Plato {item.PlatoId} no encontrado" });

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

            var orden = new Orden
            {
                UsuarioId = userId,
                Fecha = DateTime.Now,
                EstadoId = estadoId,
                TipoOrden = "LOCAL",
                MesaId = mesaIdReal, // <--- FIX: Usamos un Id válido
                TotalOrden = total,
                Detalles = nuevosDetalles
            };

            _context.Ordenes.Add(orden);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Orden enviada a cocina", id = orden.Id });
        }
    }
}