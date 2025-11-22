using megadeliciasapi.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace megadeliciasapi.Controllers
{
    [Route("api/[controller]")]        // La ruta será /api/Contabilidad
    [ApiController]
    [Authorize]                        // Igual que Plato, requiere estar logueado
    public class ContabilidadController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        // Inyectamos el contexto SOLO para mantener el mismo patrón que PlatoController,
        // aunque en este ejemplo no lo estamos usando para no depender de SQL Server.
        public ContabilidadController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. READ - Resumen contable del día actual
        // GET: api/Contabilidad/resumen-diario
        [HttpGet("resumen-diario")]
        public ActionResult<ResumenContableDto> GetResumenDiario()
        {
            var hoy = DateTime.Today;

            // ⚠ Aquí estamos devolviendo DATOS DE PRUEBA (mock),
            // para que la API funcione aunque la BD no esté configurada.
            var resumen = new ResumenContableDto
            {
                FechaInicio = hoy.ToString("yyyy-MM-dd"),
                FechaFin = hoy.ToString("yyyy-MM-dd"),
                TotalVentas = 1500.00m,
                TotalFacturado = 1500.00m,
                TotalImpuesto = 225.00m,
                Comentario = "Resumen contable simulado para el día actual."
            };

            return Ok(resumen);
        }

        // 2. READ - Resumen por rango de fechas
        // GET: api/Contabilidad/resumen-rango?fechaInicio=2025-11-01&fechaFin=2025-11-21
        [HttpGet("resumen-rango")]
        public ActionResult<ResumenContableDto> GetResumenRango(
            [FromQuery] DateTime fechaInicio,
            [FromQuery] DateTime fechaFin)
        {
            if (fechaFin < fechaInicio)
            {
                return BadRequest("La fecha fin no puede ser menor que la fecha inicio.");
            }

            // Igual: datos simulados
            var resumen = new ResumenContableDto
            {
                FechaInicio = fechaInicio.ToString("yyyy-MM-dd"),
                FechaFin = fechaFin.ToString("yyyy-MM-dd"),
                TotalVentas = 35000.00m,
                TotalFacturado = 35000.00m,
                TotalImpuesto = 5250.00m,
                Comentario = "Resumen contable simulado para el rango de fechas."
            };

            return Ok(resumen);
        }

        // 3. CREATE - Registrar un "cierre contable" (solo admin)
        // POST: api/Contabilidad/cierre
        [HttpPost("cierre")]
        [Authorize(Roles = "admin")]
        public ActionResult<CierreContableRespuestaDto> RegistrarCierre([FromBody] CierreContableSolicitudDto solicitud)
        {
            if (solicitud == null)
            {
                return BadRequest("Los datos del cierre son requeridos.");
            }

            if (solicitud.FechaCierre == default)
            {
                return BadRequest("La fecha de cierre no es válida.");
            }

            // Aquí normalmente guardarías en BD.
            // Por ahora solo simulamos la operación.
            var respuesta = new CierreContableRespuestaDto
            {
                FechaCierre = solicitud.FechaCierre.ToString("yyyy-MM-dd"),
                Usuario = string.IsNullOrWhiteSpace(solicitud.Usuario) ? "admin" : solicitud.Usuario,
                TotalVentasCerradas = 12345.67m,
                Mensaje = "Cierre contable registrado (simulado)."
            };

            // 201 Created, igual estilo que PostPlato
            return CreatedAtAction(nameof(GetResumenDiario), new { }, respuesta);
        }
    }
}