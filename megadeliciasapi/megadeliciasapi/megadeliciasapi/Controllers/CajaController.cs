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
    public class CajaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CajaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. REGISTRAR VENTA (Cobrar)
        [HttpPost("venta")]
        public async Task<IActionResult> RegistrarVenta([FromBody] object ventaData)
        {
            // Aquí implementaremos la lógica para mover una Orden a Venta
            // y registrar el MovimientoCaja.
            // Por ahora, devolvemos éxito para que el frontend no falle.
            return Ok(new { message = "Venta registrada correctamente (Simulado)" });
        }

        // 2. OBTENER MOVIMIENTOS
        [HttpGet("movimientos")]
        public async Task<IActionResult> GetMovimientos()
        {
            var movimientos = await _context.MovimientosCaja
                .Include(m => m.Usuario)
                .OrderByDescending(m => m.Fecha)
                .Take(50)
                .ToListAsync();
                
            return Ok(movimientos);
        }
    }
}