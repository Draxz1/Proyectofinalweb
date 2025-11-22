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
            // FECHA DE CORTE: Hoy a las 00:00
            var hoy = DateTime.Today;
            var estadosActivos = new[] { "PENDIENTE", "EN_PROCESO", "LISTO" };

            var ordenes = await _context.Ordenes
                .Include(o => o.Estado) 
                .Include(o => o.Detalles)
                .ThenInclude(d => d.Plato)
                // --- FILTRO OPTIMIZADO ---
                .Where(o => 
                    // 1. Mostrar SIEMPRE las activas (aunque sean de ayer a las 11:59 PM)
                    estadosActivos.Contains(o.Estado.Nombre) || 
                    // 2. Mostrar Entregados/Cancelados SOLO si son de HOY
                    o.Fecha >= hoy 
                )
                // Validación extra: evitar órdenes vacías
                .Where(o => o.Detalles.Any()) 
                .OrderByDescending(o => o.Id)
                .Select(o => new
                {
                    o.Id,
                    o.Fecha,
                    Estado = o.Estado != null ? o.Estado.Nombre : "DESCONOCIDO",
                    Mesero = "Mesero " + o.UsuarioId, 
                    Items = o.Detalles.Select(d => new { 
                        d.Cantidad, 
                        Nombre = d.Plato != null ? d.Plato.Nombre : "Plato eliminado", 
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

            var nuevoEstado = await _context.EstadosOrden.FirstOrDefaultAsync(e => e.Nombre == dto.Estado);
            if (nuevoEstado == null) return BadRequest(new { message = "Estado no válido" });

            orden.EstadoId = nuevoEstado.Id;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Estado actualizado" });
        }
    }
}