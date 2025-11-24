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

        // GET: api/cocina
        // Obtiene el "Tablero" de cocina con TODOS los estados del día actual
        [HttpGet]
        public async Task<IActionResult> GetTableroCocina()
        {
            // CAMBIO: Ahora incluimos TODOS los estados
            var estadosActivos = new[] { "PENDIENTE", "EN_PROCESO", "LISTO", "ENTREGADO", "CANCELADO" };

            // Solo mostramos órdenes del día actual para no saturar la vista
            var hoy = DateTime.Today;
            var mañana = hoy.AddDays(1);

            var ordenes = await _context.Ordenes
                .Include(o => o.Estado) 
                .Include(o => o.Detalles)
                    .ThenInclude(d => d.Plato)
                .Include(o => o.Usuario) // NUEVO: Para obtener nombre del mesero
                // Filtramos por estados y fecha
                .Where(o => estadosActivos.Contains(o.Estado.Nombre) 
                         && o.Fecha >= hoy 
                         && o.Fecha < mañana)
                .OrderBy(o => o.Fecha) // FIFO (Lo más viejo primero)
                .Select(o => new
                {
                    o.Id,
                    o.Fecha,
                    Estado = o.Estado.Nombre,
                    // MEJORADO: Nombre real del mesero
                    Mesero = o.Usuario != null ? o.Usuario.Nombre : "Mesero " + o.UsuarioId,
                    // Resumen de texto para vista rápida
                    Platos = string.Join(", ", o.Detalles.Select(d => $"{d.Cantidad}x {d.Plato.Nombre}")),
                    // Detalles completos para vista expandida
                    Items = o.Detalles.Select(d => new { 
                        d.Cantidad, 
                        Nombre = d.Plato.Nombre, 
                        Nota = d.NotaPlato 
                    })
                })
                .ToListAsync();

            return Ok(ordenes);
        }

        // GET: api/cocina/historial
        // NUEVO: Endpoint opcional para ver historial completo (últimos 7 días)
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

        // PUT: api/cocina/{id}/estado
        // Avanzar la orden en el tablero
        [HttpPut("{id}/estado")]
        public async Task<IActionResult> ActualizarEstado(int id, [FromBody] ChangeStateDto dto)
        {
            var orden = await _context.Ordenes
                .Include(o => o.Estado)
                .FirstOrDefaultAsync(o => o.Id == id);
                
            if (orden == null) 
                return NotFound(new { message = "Orden no encontrada" });

            // Buscar el ID del nuevo estado en la BD
            var nuevoEstado = await _context.EstadosOrden
                .FirstOrDefaultAsync(e => e.Nombre == dto.Estado);
            
            if (nuevoEstado == null) 
                return BadRequest(new { message = $"Estado '{dto.Estado}' no válido" });

            // Validar transiciones de estado (opcional pero recomendado)
            if (!EsTransicionValida(orden.Estado.Nombre, dto.Estado))
            {
                return BadRequest(new { 
                    message = $"No se puede cambiar de {orden.Estado.Nombre} a {dto.Estado}" 
                });
            }

            orden.EstadoId = nuevoEstado.Id;
            await _context.SaveChangesAsync();

            return Ok(new { 
                message = $"Orden #{id} movida a {dto.Estado}",
                nuevoEstado = dto.Estado
            });
        }

        // NUEVA: Validación de transiciones de estado
        private bool EsTransicionValida(string estadoActual, string nuevoEstado)
        {
            // Define las transiciones permitidas
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

        // POST: api/cocina/{id}/notificar
        // NUEVO: Endpoint para notificar al mesero
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

            // Aquí puedes implementar la notificación real:
            // - SignalR para notificación en tiempo real
            // - Email
            // - SMS
            // - Push notification
            
            // Por ahora solo registramos en logs
            Console.WriteLine($"[NOTIFICACIÓN] Orden #{id} lista para {orden.Usuario?.Nombre ?? "Mesero"}");

            // Opcional: Crear tabla de notificaciones
            // await _context.Notificaciones.AddAsync(new Notificacion { ... });
            // await _context.SaveChangesAsync();

            return Ok(new { 
                message = $"Notificación enviada a {orden.Usuario?.Nombre ?? "mesero"}",
                ordenId = id
            });
        }
    }
}