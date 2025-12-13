using megadeliciasapi.Data;
using megadeliciasapi.Models;
using megadeliciasapi.DTOs;
using megadeliciasapi.Services; // Asegúrate de tener este using
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace megadeliciasapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CocinaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly InventarioService _inventarioService; // Inyectamos el servicio

        public CocinaController(ApplicationDbContext context, InventarioService inventarioService)
        {
            _context = context;
            _inventarioService = inventarioService;
        }

        [HttpGet]
        public async Task<IActionResult> GetTableroCocina()
        {
            try 
            {
                var estadosActivos = new[] { "PENDIENTE", "EN_PROCESO", "LISTO", "ENTREGADO", "CANCELADO" };
                var hoy = DateTime.Today;
                var mañana = hoy.AddDays(1);

                var ordenes = await _context.Ordenes
                    .Include(o => o.Estado) 
                    .Include(o => o.Detalles).ThenInclude(d => d.Plato)
                    .Include(o => o.Usuario)
                    .Where(o => estadosActivos.Contains(o.Estado.Nombre) && o.Fecha >= hoy && o.Fecha < mañana)
                    .OrderBy(o => o.Fecha)
                    .Select(o => new
                    {
                        o.Id, o.Fecha, Estado = o.Estado.Nombre,
                        Mesero = o.Usuario != null ? o.Usuario.Nombre : "Mesero " + o.UsuarioId,
                        Platos = string.Join(", ", o.Detalles.Select(d => $"{d.Cantidad}x {d.Plato.Nombre}")),
                        Items = o.Detalles.Select(d => new { d.Cantidad, Nombre = d.Plato.Nombre, Nota = d.NotaPlato })
                    }).ToListAsync();

                return Ok(ordenes);
            }
            catch (Exception ex) { return StatusCode(500, new { message = "Error al obtener tablero", error = ex.Message }); }
        }

        [HttpGet("historial")]
        public async Task<IActionResult> GetHistorial([FromQuery] int dias = 7)
        {
            var fechaInicio = DateTime.Today.AddDays(-dias);
            var ordenes = await _context.Ordenes
                .Include(o => o.Estado).Include(o => o.Detalles).ThenInclude(d => d.Plato).Include(o => o.Usuario)
                .Where(o => o.Fecha >= fechaInicio)
                .OrderByDescending(o => o.Fecha)
                .Select(o => new {
                    o.Id, o.Fecha, Estado = o.Estado.Nombre,
                    Mesero = o.Usuario != null ? o.Usuario.Nombre : "Mesero " + o.UsuarioId,
                    Items = o.Detalles.Select(d => new { d.Cantidad, Nombre = d.Plato.Nombre, Nota = d.NotaPlato })
                }).ToListAsync();
            return Ok(ordenes);
        }

        [HttpPut("{id}/estado")]
        public async Task<IActionResult> ActualizarEstado(int id, [FromBody] ChangeStateDto dto)
        {
            try
            {
                var orden = await _context.Ordenes.Include(o => o.Estado).FirstOrDefaultAsync(o => o.Id == id);
                if (orden == null) return NotFound(new { message = "Orden no encontrada" });

                string estadoSolicitado = dto.Estado.ToUpper().Trim();
                
                // Validación básica de transición
                if (!EsTransicionValida(orden.Estado.Nombre, estadoSolicitado))
                    return BadRequest(new { message = $"Transición inválida de {orden.Estado.Nombre} a {estadoSolicitado}" });

                // --- LÓGICA DE INVENTARIO ---
                // Si pasa a EN_PROCESO, intentamos descontar ingredientes
                if (estadoSolicitado == "EN_PROCESO" && orden.Estado.Nombre == "PENDIENTE")
                {
                    var resultado = await _inventarioService.ProcesarConsumoOrden(id);
                    if (!resultado.Exito)
                    {
                        return BadRequest(new { message = "Stock insuficiente", detalles = resultado.Mensaje });
                    }
                }

                var nuevoEstado = await _context.EstadosOrden.FirstOrDefaultAsync(e => e.Nombre == estadoSolicitado);
                if (nuevoEstado != null)
                {
                    orden.EstadoId = nuevoEstado.Id;
                    await _context.SaveChangesAsync();
                }

                return Ok(new { message = $"Orden #{id} actualizada a {estadoSolicitado}" });
            }
            catch (Exception ex) { return StatusCode(500, new { message = "Error actualizando estado", error = ex.Message }); }
        }

        [HttpPost("{id}/notificar")]
        public async Task<IActionResult> NotificarMesero(int id)
        {
            var orden = await _context.Ordenes.Include(o => o.Usuario).Include(o => o.Estado).FirstOrDefaultAsync(o => o.Id == id);
            if (orden == null) return NotFound(new { message = "Orden no encontrada" });
            if (orden.Estado.Nombre != "LISTO") return BadRequest(new { message = "La orden no está LISTA" });
            
            return Ok(new { message = $"Notificación enviada a {orden.Usuario?.Nombre}", ordenId = id });
        }

        private bool EsTransicionValida(string actual, string nuevo)
        {
            var transiciones = new Dictionary<string, string[]> {
                { "PENDIENTE", new[] { "EN_PROCESO", "CANCELADO" } },
                { "EN_PROCESO", new[] { "LISTO", "CANCELADO" } },
                { "LISTO", new[] { "ENTREGADO", "EN_PROCESO" } },
                { "ENTREGADO", new string[] { } },
                { "CANCELADO", new string[] { } }
            };
            return transiciones.ContainsKey(actual) && transiciones[actual].Contains(nuevo);
        }
    }
}