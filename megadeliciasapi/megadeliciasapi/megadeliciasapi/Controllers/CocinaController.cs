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
        [HttpGet]
        public async Task<IActionResult> GetTableroCocina()
        {
            var hoy = DateTime.Today;

            var ordenes = await _context.Ordenes
                .Include(o => o.Estado) 
                .Include(o => o.Detalles)
                .ThenInclude(d => d.Plato)
                // CORRECCIÓN 1: Filtramos órdenes que NO tengan platos (o.Detalles.Any())
                // para que no aparezcan tarjetas vacías.
                .Where(o => (o.Fecha >= hoy || o.Estado.Nombre == "PENDIENTE" || o.Estado.Nombre == "EN_PROCESO") 
                            && o.Detalles.Any())
                .OrderByDescending(o => o.Id)
                .Select(o => new
                {
                    o.Id,
                    o.Fecha,
                    // CORRECCIÓN 2: Protección contra nulos
                    Estado = o.Estado != null ? o.Estado.Nombre : "DESCONOCIDO",
                    Mesero = "Mesero " + o.UsuarioId, 
                    Platos = string.Join(", ", o.Detalles.Select(d => $"{d.Cantidad}x {(d.Plato != null ? d.Plato.Nombre : "Eliminado")}")),
                    Items = o.Detalles.Select(d => new { 
                        d.Cantidad, 
                        // CORRECCIÓN 3: Protección si se borró el plato
                        Nombre = d.Plato != null ? d.Plato.Nombre : "Plato no encontrado", 
                        Nota = d.NotaPlato 
                    })
                })
                .ToListAsync();

            return Ok(ordenes);
        }

        // PUT: api/cocina/{id}/estado
        [HttpPut("{id}/estado")]
        public async Task<IActionResult> ActualizarEstado(int id, [FromBody] ChangeStateDto dto)
        {
            var orden = await _context.Ordenes.FindAsync(id);
            if (orden == null) return NotFound(new { message = "Orden no encontrada" });

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