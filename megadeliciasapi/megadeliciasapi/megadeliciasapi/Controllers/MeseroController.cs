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
        // Lista las órdenes recientes (Protegido contra errores nulos)
        [HttpGet("ordenes")]
        public async Task<IActionResult> ListarOrdenes()
        {
            // 1. Obtener ID del usuario de forma segura
            var userIdStr = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value 
                         ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

            var esAdmin = User.IsInRole("admin");

            var query = _context.Ordenes
                .Include(o => o.Estado)
                .Include(o => o.Detalles)
                .ThenInclude(d => d.Plato)
                .AsQueryable();

            if (!esAdmin)
            {
                query = query.Where(o => o.UsuarioId == userId);
            }

            // 2. Proyección SEGURA para evitar error 500 si hay datos corruptos
            var ordenes = await query
                .OrderByDescending(o => o.Fecha)
                .Take(20)
                .Select(o => new
                {
                    o.Id,
                    o.Fecha,
                    // Si el estado es null, muestra "Sin Estado" en vez de explotar
                    Estado = o.Estado != null ? o.Estado.Nombre : "Sin Estado", 
                    // Si un plato fue borrado, muestra "Item eliminado"
                    Resumen = string.Join(", ", o.Detalles.Select(d => 
                        $"{d.Cantidad}x {(d.Plato != null ? d.Plato.Nombre : "Item eliminado")}"
                    )),
                    Total = o.TotalOrden
                })
                .ToListAsync();

            return Ok(ordenes);
        }

        // POST: api/mesero/ordenes
        // Crear una nueva orden (Con validaciones y manejo de errores)
        [HttpPost("ordenes")]
        public async Task<IActionResult> CrearOrden([FromBody] CrearOrdenDto dto)
        {
            try
            {
                if (dto.Detalles == null || !dto.Detalles.Any())
                    return BadRequest(new { message = "La orden debe tener al menos un plato." });

                // 1. OBTENER Y VALIDAR USUARIO
                var userIdStr = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value 
                             ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (!int.TryParse(userIdStr, out int userId) || userId <= 0)
                {
                    return Unauthorized(new { message = "Token inválido: No se pudo identificar al usuario." });
                }

                // Verificar que el usuario exista realmente en la BD
                var usuarioExiste = await _context.Usuarios.AnyAsync(u => u.Id == userId);
                if (!usuarioExiste)
                {
                    return Unauthorized(new { message = $"El usuario con ID {userId} no existe en la base de datos. Cierre sesión e intente de nuevo." });
                }

                // 2. VALIDAR OBTENCIÓN DEL ESTADO 'PENDIENTE'
                var estadoPendiente = await _context.EstadosOrden.FirstOrDefaultAsync(e => e.Nombre == "PENDIENTE");
                if (estadoPendiente == null)
                {
                    // Intentar fallback al ID 1 si no existe por nombre
                    var estadoDefault = await _context.EstadosOrden.FindAsync(1);
                    if (estadoDefault == null)
                        return BadRequest(new { message = "Error crítico: No existen estados de orden configurados (ni 'PENDIENTE' ni ID 1)." });
                    
                    estadoPendiente = estadoDefault;
                }

                // 3. VALIDAR MESA
                int mesaIdReal;
                if (dto.MesaId.HasValue && dto.MesaId > 0)
                {
                    mesaIdReal = dto.MesaId.Value;
                }
                else
                {
                    var primeraMesa = await _context.Mesas.FirstOrDefaultAsync();
                    if (primeraMesa == null) 
                        return BadRequest(new { message = "No hay mesas registradas en el sistema." });
                    mesaIdReal = primeraMesa.Id;
                }

                // 4. PROCESAR DETALLES Y CALCULAR TOTAL
                decimal total = 0;
                var nuevosDetalles = new List<DetalleOrden>();

                foreach (var item in dto.Detalles)
                {
                    var plato = await _context.Platos.FindAsync(item.PlatoId);
                    if (plato == null) return BadRequest(new { message = $"El plato con ID {item.PlatoId} no existe." });

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

                // 5. CREAR LA ORDEN (Usando namespace explícito para evitar conflictos)
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

                return Ok(new { message = "Orden enviada a cocina correctamente", id = orden.Id });
            }
            catch (DbUpdateException dbEx)
            {
                // Captura errores específicos de Base de Datos (Foreign Keys, etc.)
                var errorMsg = dbEx.InnerException?.Message ?? dbEx.Message;
                return StatusCode(500, new { message = "Error de base de datos al guardar la orden.", detalle = errorMsg });
            }
            catch (Exception ex)
            {
                // Captura cualquier otro error inesperado
                return StatusCode(500, new { message = "Ocurrió un error inesperado en el servidor.", error = ex.Message });
            }
        }
    }
}