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
    [Authorize(Roles = "admin,cocinero")] // Solo personal autorizado
    public class CocinaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CocinaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/cocina
        // Obtiene el "Tablero" de cocina (Órdenes activas)
        [HttpGet]
        public async Task<IActionResult> GetTableroCocina()
        {
            // Solo nos interesan estos estados
            var estadosActivos = new[] { "PENDIENTE", "EN_PROCESO", "LISTO" };

            var ordenes = await _context.Ordenes
                .Include(o => o.Estado) 
                .Include(o => o.Detalles)
                .ThenInclude(d => d.Plato)
                // Filtramos por los nombres de estado
                .Where(o => estadosActivos.Contains(o.Estado.Nombre)) 
                .OrderBy(o => o.Fecha) // FIFO (Lo más viejo primero)
                .Select(o => new
                {
                    o.Id,
                    o.Fecha,
                    Estado = o.Estado.Nombre,
                    Mesero = "Mesero " + o.UsuarioId, // Opcional: Podrías incluir el nombre real
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

        // PUT: api/cocina/{id}/estado
        // Avanzar la orden en el tablero
        [HttpPut("{id}/estado")]
        public async Task<IActionResult> ActualizarEstado(int id, [FromBody] ChangeStateDto dto)
        {
            var orden = await _context.Ordenes.FindAsync(id);
            if (orden == null) return NotFound(new { message = "Orden no encontrada" });

            // Buscar el ID del nuevo estado en la BD
            var nuevoEstado = await _context.EstadosOrden
                                    .FirstOrDefaultAsync(e => e.Nombre == dto.Estado);
            
            if (nuevoEstado == null) 
                return BadRequest(new { message = $"Estado '{dto.Estado}' no válido" });

            orden.EstadoId = nuevoEstado.Id;
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Orden #{id} movida a {dto.Estado}" });
        }
    }
}