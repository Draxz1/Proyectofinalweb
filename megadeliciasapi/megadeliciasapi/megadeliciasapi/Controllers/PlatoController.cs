using megadeliciasapi.Data;
using megadeliciasapi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using megadeliciasapi.DTOs;
namespace megadeliciasapi.Controllers
{
    [Route("api/[controller]")] // La ruta será /api/Plato
    [ApiController]
    [Authorize] // Requiere estar logueado para ver el menú
    public class PlatoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PlatoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. READ ALL (Obtener todo el menú)
        // GET: api/Plato
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Plato>>> GetPlatos()
        {
            return await _context.Platos
                                 .OrderBy(p => p.Categoria) // Ordenamos por categoría para que se vea mejor
                                 .ThenBy(p => p.Nombre)
                                 .ToListAsync();
        }

        // 2. READ BY ID (Obtener un plato específico)
        // GET: api/Plato/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Plato>> GetPlato(int id)
        {
            var plato = await _context.Platos.FindAsync(id);

            if (plato == null)
            {
                return NotFound();
            }

            return plato;
        }

        // 3. CREATE (Crear plato - Solo Admin)
        // POST: api/Plato
        [HttpPost]
        [Authorize(Roles = "admin")] 
        public async Task<ActionResult<Plato>> PostPlato(Plato plato)
        {
            if (plato == null) return BadRequest();

            plato.Id = 0; // Aseguramos que se cree uno nuevo
            plato.CreadoEn = DateTime.Now;
            
            _context.Platos.Add(plato);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPlato), new { id = plato.Id }, plato);
        }

        // 4. UPDATE (Editar plato - Solo Admin)
        // PUT: api/Plato/5
        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> PutPlato(int id, Plato plato)
        {
            if (id != plato.Id)
            {
                return BadRequest("El ID de la URL no coincide con el del cuerpo.");
            }

            _context.Entry(plato).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Platos.Any(e => e.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // 5. DELETE (Borrar plato - Solo Admin)
        // DELETE: api/Plato/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeletePlato(int id)
        {
            var plato = await _context.Platos.FindAsync(id);
            if (plato == null)
            {
                return NotFound();
            }

            _context.Platos.Remove(plato);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("{id}/ingredientes")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> AgregarIngredientes(int id, [FromBody] List<AgregarIngredienteDto> ingredientes)
        {
            var plato = await _context.Platos.FindAsync(id);
            if (plato == null) return NotFound(new { message = "Plato no encontrado" });

            foreach (var ing in ingredientes)
            {
                var itemExiste = await _context.InventarioItems.AnyAsync(i => i.Id == ing.ItemId);
                if (!itemExiste) return BadRequest(new { message = $"El ingrediente {ing.ItemId} no existe" });

                var yaExiste = await _context.PlatoIngredientes
                    .AnyAsync(pi => pi.PlatoId == id && pi.ItemId == ing.ItemId);

                if (yaExiste) continue;

                _context.PlatoIngredientes.Add(new PlatoIngrediente
                {
                    PlatoId = id,
                    ItemId = ing.ItemId,
                    CantidadUsada = ing.Cantidad,
                    UnidadMedida = ing.UnidadMedida
                });
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Ingredientes agregados correctamente" });
        }
    }
}