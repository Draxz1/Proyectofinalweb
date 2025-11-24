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
    var estadosValidos = new[] { "LISTO", "ENTREGADO", "EN_PROCESO", "PENDIENTE", "CANCELADO" }; 

    // Mostrar todas las órdenes sin venta, sin importar el estado
    var ordenes = await _context.Ordenes
    .Where(o => o.Venta == null)
    .Include(o => o.Estado) // Aseguramos cargar el estado
    .Select(o => new
    {
        o.Id,
        o.MesaId,
        MeseroNombre = o.Usuario.Nombre,
        Total = o.TotalOrden,
        FechaCreacion = o.Fecha,
        Estado = o.Estado.Nombre ?? "SIN ESTADO" // Si no tiene estado, muestra "SIN ESTADO"
    })
    .ToListAsync();

    return Ok(ordenes);
}

        [HttpGet("metodos-pago")]
            [AllowAnonymous] 
            public async Task<IActionResult> GetMetodosPago()
        {
        var metodos = await _context.MetodosPago
            .Select(m => new { m.Id, m.Nombre })
            .ToListAsync();
        return Ok(metodos);
        }

        // 4. REGISTRAR PAGO DE UNA ORDEN (crea Venta + Pago)
        [HttpPost("ordenes/{id}/pagar")]
        public async Task<IActionResult> RegistrarPago(int id, [FromBody] RegistrarPagoDto dto)
        {
            // Validar método de pago
            var metodoPago = await _context.MetodosPago
                .FirstOrDefaultAsync(m => m.Nombre == dto.MetodoPago);
            if (metodoPago == null)
                return BadRequest(new { message = "Método de pago no válido." });

            // Validar orden
            var orden = await _context.Ordenes
                .Include(o => o.Estado)
                .FirstOrDefaultAsync(o => o.Id == id && o.Venta == null);

            if (orden == null)
                return BadRequest(new { message = "Orden no encontrada o ya pagada." });

            var estadosValidos = new[] { "LISTO", "ENTREGADO" };
            if (!estadosValidos.Contains(orden.Estado.Nombre))
                return BadRequest(new { message = "La orden no está lista para ser pagada." });

            // Crear Venta
            var venta = new Venta
            {
                OrdenId = orden.Id,
                UsuarioId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!),
                TotalVenta = orden.TotalOrden,
                Fecha = DateTime.Now
            };

            _context.Ventas.Add(venta);
            await _context.SaveChangesAsync();

            // Crear Pago
            var pago = new Pago
            {
                VentaId = venta.Id,
                MetodoPagoId = metodoPago.Id,
                MontoPago = orden.TotalOrden,
                Estado = "completado",
                FechaPago = DateTime.Now
            };

            _context.Pagos.Add(pago);
            await _context.SaveChangesAsync();

            // Opcional: registrar MovimientoCaja asociado al pago (si quieres llevar control de caja por movimiento)
            var movimiento = new MovimientoCaja
            {
                UsuarioId = venta.UsuarioId,
                Tipo = "INGRESO",
                Monto = orden.TotalOrden,
                MetodoPagoId = metodoPago.Id,
                Descripcion = $"Pago Orden {orden.Id}",
                PagoId = pago.Id,
                Fecha = DateTime.Now
            };
            _context.MovimientosCaja.Add(movimiento);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Pago registrado correctamente." });
        }

        // 5. CIERRE DIARIO 
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

        // 6. HISTORIAL DE VENTAS/PAGOS 
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
        // NUEVOS ENDPOINTS: CIERRES
        // -------------------------

        // POST: api/Caja/cierres  -> crear un cierre (snapshot) y asociar movimientos
        [HttpPost("cierres")]
        public async Task<IActionResult> CrearCierre([FromBody] CrearCierreDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (dto.Hasta < dto.Desde) return BadRequest(new { message = "El rango de fechas no es válido" });

            // 1) Traer movimientos en el rango que aún no estén asignados a un cierre
            var movimientos = await _context.MovimientosCaja
                .Where(m => m.Fecha >= dto.Desde && m.Fecha <= dto.Hasta && m.CierreId == null)
                .Include(m => m.MetodoPago)
                .ToListAsync();

            // 2) Intentar resolver MetodoPagoId para Efectivo/Tarjeta/Transferencia por nombre (case-insensitive)
            var metodos = await _context.MetodosPago.ToListAsync();

            int? idEfectivo = metodos.FirstOrDefault(x => x.Nombre != null && x.Nombre.Trim().ToLower().Contains("efectivo"))?.Id;
            int? idTarjeta = metodos.FirstOrDefault(x => x.Nombre != null && (x.Nombre.Trim().ToLower().Contains("tarjeta") || x.Nombre.Trim().ToLower().Contains("pos")))?.Id;
            int? idTransferencia = metodos.FirstOrDefault(x => x.Nombre != null && x.Nombre.Trim().ToLower().Contains("transfer"))?.Id;

            // 3) Calcular totales por metodo (si id no existe, quedará en 0 y se hará fallback por nombre)
            decimal totalEfectivo = movimientos
                .Where(m => idEfectivo.HasValue ? m.MetodoPagoId == idEfectivo.Value : false)
                .Sum(m => m.Monto);

            decimal totalTarjetas = movimientos
                .Where(m => idTarjeta.HasValue ? m.MetodoPagoId == idTarjeta.Value : false)
                .Sum(m => m.Monto);

            decimal totalTransferencias = movimientos
                .Where(m => idTransferencia.HasValue ? m.MetodoPagoId == idTransferencia.Value : false)
                .Sum(m => m.Monto);

            // 4) Fallback: agrupar por nombre si no se detectaron ids
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

            // 5) Crear CierreCaja
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

            // 6) Asociar movimientos al cierre (marcar CierreId)
            foreach (var m in movimientos)
            {
                m.CierreId = cierre.Id;
            }
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCierrePorId), new { id = cierre.Id }, cierre);
        }

        // GET: api/Caja/cierres
        [HttpGet("cierres")]
        public async Task<IActionResult> ListarCierres()
        {
            var lista = await _context.CierresCaja
                .Include(c => c.Usuario)
                .OrderByDescending(c => c.CreadoEn)
                .ToListAsync();

            return Ok(lista);
        }

        // GET: api/Caja/cierres/{id}
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

    // DTOs auxiliares para este controlador:
    public class RegistrarPagoDto
    {
        public string MetodoPago { get; set; } = string.Empty;
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
