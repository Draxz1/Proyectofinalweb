using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            return Ok(new { message = "Venta registrada correctamente (Simulado)" });
        }

        // 2. OBTENER MOVIMIENTOS
        [HttpGet("movimientos")]
        public async Task<IActionResult> GetMovimientos()
        {
            var movimientos = await _context.MovimientosCaja
                .Include(m => m.Usuario)
                .Include(m => m.MetodoPago)
                .OrderByDescending(m => m.Fecha)
                .Take(50)
                .ToListAsync();

            return Ok(movimientos);
        }

        // 3. OBTENER ÓRDENES PENDIENTES DE PAGO 
        [HttpGet("ordenes-pendientes")]
        public async Task<IActionResult> GetOrdenesPendientes()
        {
            // ✅ Mostrar todas las órdenes sin venta (no pagadas)
            var ordenes = await _context.Ordenes
                .Where(o => o.Venta == null) // Solo órdenes sin venta
                .Include(o => o.Estado)
                .Include(o => o.Usuario) // ✅ CRÍTICO: Incluir usuario
                .OrderByDescending(o => o.Fecha)
                .Select(o => new
                {
                    o.Id,
                    o.MesaId,
                    MeseroNombre = o.Usuario != null ? o.Usuario.Nombre : "Desconocido",
                    Total = o.TotalOrden,
                    FechaCreacion = o.Fecha,
                    Estado = o.Estado != null ? o.Estado.Nombre : "PENDIENTE"
                })
                .ToListAsync();

            return Ok(ordenes);
        }

        // 4. OBTENER MÉTODOS DE PAGO
        [HttpGet("metodos-pago")]
        [AllowAnonymous] 
        public async Task<IActionResult> GetMetodosPago()
        {
            var metodos = await _context.MetodosPago
                .Where(m => m.Activo) // ✅ Solo métodos activos
                .Select(m => new { m.Id, m.Nombre })
                .ToListAsync();
            
            return Ok(metodos);
        }

        // 5. ✅ REGISTRAR PAGO DE UNA ORDEN (CORREGIDO)
        [HttpPost("ordenes/{id}/pagar")]
        public async Task<IActionResult> RegistrarPago(int id, [FromBody] RegistrarPagoDto dto)
        {
            try
            {
                // ✅ 1. Validar que la orden existe y no está pagada
                var orden = await _context.Ordenes
                    .Include(o => o.Estado)
                    .Include(o => o.Usuario)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (orden == null)
                    return NotFound(new { message = "Orden no encontrada" });

                if (orden.Venta != null)
                    return BadRequest(new { message = "Esta orden ya fue pagada" });

                // ✅ 2. Validar método de pago POR ID (más robusto)
                var metodoPago = await _context.MetodosPago
                    .FirstOrDefaultAsync(m => m.Id == dto.MetodoPagoId && m.Activo);

                if (metodoPago == null)
                    return BadRequest(new { message = "Método de pago no válido o inactivo" });

                // ✅ 3. Validar monto (opcional, pero recomendado)
                if (dto.Monto <= 0)
                    return BadRequest(new { message = "El monto debe ser mayor a 0" });

                if (dto.Monto != orden.TotalOrden)
                    return BadRequest(new { message = $"El monto ({dto.Monto}) no coincide con el total de la orden ({orden.TotalOrden})" });

                // ✅ 4. Obtener usuario actual del token JWT
                var usuarioIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(usuarioIdClaim))
                    return Unauthorized(new { message = "Usuario no autenticado" });

                var usuarioId = int.Parse(usuarioIdClaim);

                // ✅ 5. Crear Venta
                var venta = new Venta
                {
                    OrdenId = orden.Id,
                    UsuarioId = usuarioId,
                    TotalVenta = orden.TotalOrden,
                    Fecha = DateTime.Now
                };

                _context.Ventas.Add(venta);
                await _context.SaveChangesAsync(); // Guardar para obtener venta.Id

                // ✅ 6. Crear Pago
                var pago = new Pago
                {
                    VentaId = venta.Id,
                    MetodoPagoId = metodoPago.Id,
                    MontoPago = dto.Monto,
                    Estado = "completado",
                    FechaPago = DateTime.Now
                };

                _context.Pagos.Add(pago);
                await _context.SaveChangesAsync(); // Guardar para obtener pago.Id

                // ✅ 7. Crear MovimientoCaja (INGRESO)
                var movimiento = new MovimientoCaja
                {
                    UsuarioId = usuarioId,
                    Tipo = "INGRESO",
                    Monto = dto.Monto,
                    MetodoPagoId = metodoPago.Id,
                    Descripcion = $"Pago Orden #{orden.Id} - Mesa {orden.MesaId}",
                    PagoId = pago.Id,
                    Fecha = DateTime.Now
                };
                _context.MovimientosCaja.Add(movimiento);

                // ✅ 8. Cambiar estado de la orden a ENTREGADO (opcional)
                var estadoEntregado = await _context.EstadosOrden  
                    .FirstOrDefaultAsync(e => e.Nombre == "ENTREGADO");
                
                if (estadoEntregado != null)
                {
                    orden.EstadoId = estadoEntregado.Id;
                    _context.Ordenes.Update(orden);
                }

                // ✅ 9. Guardar todos los cambios
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Pago registrado correctamente",
                    ventaId = venta.Id,
                    pagoId = pago.Id,
                    movimientoId = movimiento.Id,
                    ordenId = orden.Id,
                    metodoPago = metodoPago.Nombre,
                    monto = dto.Monto
                });
            }
            catch (Exception ex)
            {
                // ✅ Log detallado del error (solo en desarrollo)
                Console.WriteLine($"❌ ERROR en RegistrarPago: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");

                return StatusCode(500, new
                {
                    message = "Error interno al registrar el pago",
                    error = ex.Message
                });
            }
        }

        // 6. CIERRE DIARIO 
        [HttpGet("cierre-diario")]
        public async Task<IActionResult> GetCierreDiario()
        {
            var hoy = DateOnly.FromDateTime(DateTime.Now);
            var pagos = await _context.Pagos
                .Include(p => p.MetodoPago)
                .Where(p => DateOnly.FromDateTime(p.FechaPago) == hoy)
                .ToListAsync();

            var efectivo = pagos.Where(p => p.MetodoPago != null && p.MetodoPago.Nombre == "Efectivo").Sum(p => p.MontoPago);
            var tarjeta = pagos.Where(p => p.MetodoPago != null && p.MetodoPago.Nombre == "Tarjeta").Sum(p => p.MontoPago);
            var transferencia = pagos.Where(p => p.MetodoPago != null && p.MetodoPago.Nombre == "Transferencia").Sum(p => p.MontoPago);

            return Ok(new
            {
                TotalEfectivo = efectivo,
                TotalTarjeta = tarjeta,
                TotalTransferencia = transferencia,
                TotalGeneral = pagos.Sum(p => p.MontoPago),
                CantidadOrdenes = pagos.Select(p => p.VentaId).Distinct().Count()
            });
        }

        // 7. HISTORIAL DE VENTAS/PAGOS 
        [HttpGet("historial")]
        public async Task<IActionResult> GetHistorial([FromQuery] string fecha, [FromQuery] string metodoPago = "Todos")
        {
            var fechaFiltro = DateOnly.Parse(fecha);
            var query = _context.Pagos
                .Include(p => p.MetodoPago)
                .Include(p => p.Venta)
                    .ThenInclude(v => v.Orden)
                        .ThenInclude(o => o.Usuario)
                .Where(p => DateOnly.FromDateTime(p.FechaPago) == fechaFiltro);

            if (metodoPago != "Todos")
            {
                var metodo = await _context.MetodosPago.FirstOrDefaultAsync(m => m.Nombre == metodoPago);
                if (metodo != null)
                    query = query.Where(p => p.MetodoPagoId == metodo.Id);
                else
                    return Ok(new List<object>());
            }

            var resultados = await query
                .Select(p => new
                {
                    OrdenId = p.Venta.OrdenId,
                    MesaId = p.Venta.Orden.MesaId,
                    MeseroNombre = p.Venta.Orden.Usuario.Nombre,
                    Total = p.MontoPago,
                    MetodoPago = p.MetodoPago.Nombre,
                    FechaCreacion = p.FechaPago
                })
                .ToListAsync();

            return Ok(resultados);
        }

        // -------------------------
        // ENDPOINTS DE CIERRES
        // -------------------------

        [HttpPost("cierres")]
        public async Task<IActionResult> CrearCierre([FromBody] CrearCierreDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (dto.Hasta < dto.Desde) return BadRequest(new { message = "El rango de fechas no es válido" });

            var movimientos = await _context.MovimientosCaja
                .Where(m => m.Fecha >= dto.Desde && m.Fecha <= dto.Hasta && m.CierreId == null)
                .Include(m => m.MetodoPago)
                .ToListAsync();

            var metodos = await _context.MetodosPago.ToListAsync();

            int? idEfectivo = metodos.FirstOrDefault(x => x.Nombre != null && x.Nombre.Trim().ToLower().Contains("efectivo"))?.Id;
            int? idTarjeta = metodos.FirstOrDefault(x => x.Nombre != null && (x.Nombre.Trim().ToLower().Contains("tarjeta") || x.Nombre.Trim().ToLower().Contains("pos")))?.Id;
            int? idTransferencia = metodos.FirstOrDefault(x => x.Nombre != null && x.Nombre.Trim().ToLower().Contains("transfer"))?.Id;

            decimal totalEfectivo = movimientos
                .Where(m => idEfectivo.HasValue ? m.MetodoPagoId == idEfectivo.Value : false)
                .Sum(m => m.Monto);

            decimal totalTarjetas = movimientos
                .Where(m => idTarjeta.HasValue ? m.MetodoPagoId == idTarjeta.Value : false)
                .Sum(m => m.Monto);

            decimal totalTransferencias = movimientos
                .Where(m => idTransferencia.HasValue ? m.MetodoPagoId == idTransferencia.Value : false)
                .Sum(m => m.Monto);

            if (!idEfectivo.HasValue || !idTarjeta.HasValue || !idTransferencia.HasValue)
            {
                var byName = movimientos
                    .Where(m => m.MetodoPago != null)
                    .GroupBy(m => m.MetodoPago!.Nombre?.Trim().ToLower())
                    .ToDictionary(g => g.Key ?? "", g => g.Sum(m => m.Monto));

                if (!idEfectivo.HasValue)
                {
                    var key = byName.Keys.FirstOrDefault(k => k.Contains("efectivo"));
                    if (key != null) totalEfectivo = byName[key];
                }
                if (!idTarjeta.HasValue)
                {
                    var key = byName.Keys.FirstOrDefault(k => k.Contains("tarjeta") || k.Contains("pos"));
                    if (key != null) totalTarjetas = byName[key];
                }
                if (!idTransferencia.HasValue)
                {
                    var key = byName.Keys.FirstOrDefault(k => k.Contains("transfer"));
                    if (key != null) totalTransferencias = byName[key];
                }
            }

            var cierre = new CierreCaja
            {
                UsuarioId = dto.UsuarioId,
                Desde = dto.Desde,
                Hasta = dto.Hasta,
                TotalEfectivo = totalEfectivo,
                TotalTarjetas = totalTarjetas,
                TotalTransferencias = totalTransferencias,
                EfectivoContado = dto.EfectivoContado,
                Diferencia = dto.EfectivoContado - totalEfectivo,
                Observaciones = dto.Observaciones,
                CreadoEn = DateTime.Now
            };

            await _context.CierresCaja.AddAsync(cierre);
            await _context.SaveChangesAsync();

            foreach (var m in movimientos)
            {
                m.CierreId = cierre.Id;
            }
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCierrePorId), new { id = cierre.Id }, cierre);
        }

        [HttpGet("cierres")]
        public async Task<IActionResult> ListarCierres()
        {
            var lista = await _context.CierresCaja
                .Include(c => c.Usuario)
                .OrderByDescending(c => c.CreadoEn)
                .ToListAsync();

            return Ok(lista);
        }

        [HttpGet("cierres/{id}")]
        public async Task<IActionResult> GetCierrePorId(int id)
        {
            var cierre = await _context.CierresCaja
                .Include(c => c.Usuario)
                .Include(c => c.MovimientosIncluidos)
                    .ThenInclude(m => m.MetodoPago)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cierre == null) return NotFound();
            return Ok(cierre);
        }
    }

    // ✅ DTOs ACTUALIZADOS
    public class RegistrarPagoDto
    {
        public int MetodoPagoId { get; set; } // ✅ Ahora recibe el ID
        public decimal Monto { get; set; }     // ✅ Agregado para validación
    }

    public class CrearCierreDto
    {
        public int UsuarioId { get; set; }
        public DateTime Desde { get; set; }
        public DateTime Hasta { get; set; }
        public decimal EfectivoContado { get; set; }
        public string? Observaciones { get; set; }
    }
}