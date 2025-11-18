using megadeliciasapi.Data; // <-- Importa el DbContext
using megadeliciasapi.Models; // <-- Importa el modelo 'Plato'
using Microsoft.AspNetCore.Authorization; // <-- Para proteger el endpoint
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // <-- Para usar .ToListAsync()

namespace megadeliciasapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // <-- ¡Protege todo el controlador! Nadie sin token puede ver esto.
    public class PlatosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        // 1. Inyecta la base de datos (DbContext)
        public PlatosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 2. Crea el método GET para obtener todos los platos
        // Esta función se activará con: GET /api/Platos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Plato>>> GetPlatos()
        {
            // 3. Lee la tabla 'Platos' de la BD y la convierte en una lista
            var platos = await _context.Platos
                                .OrderBy(p => p.Nombre) // Opcional: ordenarlos
                                .ToListAsync();

            // 4. Devuelve los platos como JSON con un código 200 OK
            return Ok(platos);
        }

        // (Aquí pondremos los métodos POST, PUT, DELETE más adelante en el proyecto)
    }
}