using megadeliciasapi.Data;
using megadeliciasapi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace megadeliciasapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MesaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MesaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Mesa
        // Obtiene todas las mesas para que el mesero elija
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Mesa>>> GetMesas()
        {
            return await _context.Mesas
                                 .OrderBy(m => m.Id) // O por código
                                 .ToListAsync();
        }

        // (Opcional) Endpoint para liberar una mesa manualmente si se necesita
        [HttpPut("{id}/liberar")]
        public async Task<IActionResult> LiberarMesa(int id)
        {
            var mesa = await _context.Mesas.FindAsync(id);
            if (mesa == null) return NotFound();

            mesa.Activa = true; // O el campo que uses para estado (ej. "Disponible")
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Mesa {mesa.Codigo} liberada." });
        }
    }
}