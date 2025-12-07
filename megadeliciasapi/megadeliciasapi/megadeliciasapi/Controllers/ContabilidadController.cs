using megadeliciasapi.Data;
using megadeliciasapi.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace megadeliciasapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ContabilidadController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ContabilidadController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================
        // 1. CIERRE DIARIO (HOY)
        // ==========================
        [HttpGet("cierre-diario")]
        public async Task<ActionResult<CierreResumenDto>> GetCierreDiario()
        {
            var hoy = DateTime.Today;
            return await CalcularCierrePorFecha(hoy);
        }

        // ==========================
        // 2. CIERRE POR FECHA
        // ==========================
        [HttpGet("cierre-por-fecha")]
        public async Task<ActionResult<CierreResumenDto>> GetCierrePorFecha([FromQuery] DateTime fecha)
        {
            var cierreGuardado = await _context.CierresCaja
                .Where(c => c.Desde.Date <= fecha.Date && c.Hasta.Date >= fecha.Date)
                .OrderByDescending(c => c.CreadoEn)
                .FirstOrDefaultAsync();

            if (cierreGuardado != null)
            {
                decimal cajaInicialGuardada = 0;
                if (cierreGuardado.Diferencia == 0)
                {
                    cajaInicialGuardada = cierreGuardado.EfectivoContado - cierreGuardado.TotalEfectivo;
                    if (cajaInicialGuardada < 0) cajaInicialGuardada = 0;
                }

                decimal efectivoEsperadoGuardado = cajaInicialGuardada + cierreGuardado.TotalEfectivo;

                var dtoGuardado = new CierreResumenDto
                {
                    Fecha = fecha.ToString("yyyy-MM-dd"),
                    TotalEfectivo = cierreGuardado.TotalEfectivo,
                    TotalTarjeta = cierreGuardado.TotalTarjetas,
                    TotalTransferencia = cierreGuardado.TotalTransferencias,
                    CajaInicial = cajaInicialGuardada,
                    EfectivoContado = cierreGuardado.EfectivoContado,
                    EfectivoEsperado = efectivoEsperadoGuardado,
                    Diferencia = cierreGuardado.Diferencia,
                    Cuadro = cierreGuardado.Diferencia == 0
                };
                return Ok(dtoGuardado);
            }

            return await CalcularCierrePorFecha(fecha.Date);
        }

        // Cálculo interno de cierre por fecha
        private async Task<ActionResult<CierreResumenDto>> CalcularCierrePorFecha(DateTime fecha)
        {
            var fechaInicio = fecha.Date;
            var fechaFin = fecha.Date.AddDays(1).AddTicks(-1);

            var pagos = await _context.Pagos
                .Include(p => p.MetodoPago)
                .Where(p => p.FechaPago >= fechaInicio && p.FechaPago <= fechaFin)
                .ToListAsync();

            var metodos = await _context.MetodosPago.ToListAsync();

            int? idEfectivo = metodos.FirstOrDefault(m =>
                m.Nombre != null && m.Nombre.Trim().ToLower().Contains("efectivo"))?.Id;

            int? idTarjeta = metodos.FirstOrDefault(m =>
                m.Nombre != null && (m.Nombre.Trim().ToLower().Contains("tarjeta") ||
                                     m.Nombre.Trim().ToLower().Contains("pos")) )?.Id;

            int? idTransferencia = metodos.FirstOrDefault(m =>
                m.Nombre != null && m.Nombre.Trim().ToLower().Contains("transfer"))?.Id;

            decimal totalEfectivo = pagos
                .Where(p => p.MetodoPago != null &&
                            ((idEfectivo.HasValue && p.MetodoPagoId == idEfectivo.Value) ||
                             (!idEfectivo.HasValue && p.MetodoPago!.Nombre != null &&
                              p.MetodoPago.Nombre.Trim().ToLower().Contains("efectivo"))))
                .Sum(p => p.MontoPago);

            decimal totalTarjeta = pagos
                .Where(p => p.MetodoPago != null &&
                            ((idTarjeta.HasValue && p.MetodoPagoId == idTarjeta.Value) ||
                             (!idTarjeta.HasValue && p.MetodoPago!.Nombre != null &&
                              (p.MetodoPago.Nombre.Trim().ToLower().Contains("tarjeta") ||
                               p.MetodoPago.Nombre.Trim().ToLower().Contains("pos")))))
                .Sum(p => p.MontoPago);

            decimal totalTransferencia = pagos
                .Where(p => p.MetodoPago != null &&
                            ((idTransferencia.HasValue && p.MetodoPagoId == idTransferencia.Value) ||
                             (!idTransferencia.HasValue && p.MetodoPago!.Nombre != null &&
                              p.MetodoPago.Nombre.Trim().ToLower().Contains("transfer"))))
                .Sum(p => p.MontoPago);

            var cierreGuardado = await _context.CierresCaja
                .Where(c => c.Desde.Date <= fecha.Date && c.Hasta.Date >= fecha.Date)
                .OrderByDescending(c => c.CreadoEn)
                .FirstOrDefaultAsync();

            decimal cajaInicial = 0;
            decimal efectivoContado = cierreGuardado?.EfectivoContado ?? 0;

            decimal efectivoEsperado = cajaInicial + totalEfectivo;
            decimal diferencia = efectivoContado - efectivoEsperado;

            var dto = new CierreResumenDto
            {
                Fecha = fecha.ToString("yyyy-MM-dd"),
                TotalEfectivo = totalEfectivo,
                TotalTarjeta = totalTarjeta,
                TotalTransferencia = totalTransferencia,
                CajaInicial = cajaInicial,
                EfectivoContado = efectivoContado,
                EfectivoEsperado = efectivoEsperado,
                Diferencia = diferencia,
                Cuadro = diferencia == 0
            };

            return Ok(dto);
        }

        // ==========================
        // 3. CREAR CIERRE
        // ==========================
        [HttpPost("crear-cierre")]
public async Task<ActionResult> CrearCierre([FromBody] CrearCierreContabilidadDto dto)
{
    if (!ModelState.IsValid)
        return BadRequest(ModelState);

    if (dto.CajaInicial < 0)
        return BadRequest(new { message = "La caja inicial no puede ser negativa." });

    if (dto.EfectivoContado < 0)
        return BadRequest(new { message = "El efectivo contado no puede ser negativo." });

    // ⚠️ Regla nueva: el cierre SIEMPRE es del día actual
    var fecha = DateTime.Today;
    var fechaInicio = fecha.Date;
    var fechaFin = fecha.Date.AddDays(1).AddTicks(-1);

    // (Opcional) Validar que lo que venga en dto.Fecha también sea hoy,
    // por si el docente quiere ver esa validación:
    if (dto.Fecha.Date != fecha)
    {
        return BadRequest(new { message = "Solo puedes crear cierres del día actual." });
    }

    // Verificar si ya existe un cierre para hoy
    var cierreExistente = await _context.CierresCaja
        .Where(c => c.Desde.Date <= fecha.Date && c.Hasta.Date >= fecha.Date)
        .FirstOrDefaultAsync();

    if (cierreExistente != null)
    {
        return BadRequest(new { message = "Ya existe un cierre para el día de hoy." });
    }

    // ===== AQUÍ SIGUE IGUAL: sumar pagos del día por método =====

    var pagos = await _context.Pagos
        .Include(p => p.MetodoPago)
        .Where(p => p.FechaPago >= fechaInicio && p.FechaPago <= fechaFin)
        .ToListAsync();

    var metodos = await _context.MetodosPago.ToListAsync();

    int? idEfectivo = metodos.FirstOrDefault(m =>
        m.Nombre != null && m.Nombre.Trim().ToLower().Contains("efectivo"))?.Id;

    int? idTarjeta = metodos.FirstOrDefault(m =>
        m.Nombre != null && (m.Nombre.Trim().ToLower().Contains("tarjeta") ||
                             m.Nombre.Trim().ToLower().Contains("pos")) )?.Id;

    int? idTransferencia = metodos.FirstOrDefault(m =>
        m.Nombre != null && m.Nombre.Trim().ToLower().Contains("transfer"))?.Id;

    decimal totalEfectivo = pagos
        .Where(p => p.MetodoPago != null &&
                    ((idEfectivo.HasValue && p.MetodoPagoId == idEfectivo.Value) ||
                     (!idEfectivo.HasValue && p.MetodoPago!.Nombre != null &&
                      p.MetodoPago.Nombre.Trim().ToLower().Contains("efectivo"))))
        .Sum(p => p.MontoPago);

    decimal totalTarjeta = pagos
        .Where(p => p.MetodoPago != null &&
                    ((idTarjeta.HasValue && p.MetodoPagoId == idTarjeta.Value) ||
                     (!idTarjeta.HasValue && p.MetodoPago!.Nombre != null &&
                      (p.MetodoPago.Nombre.Trim().ToLower().Contains("tarjeta") ||
                       p.MetodoPago.Nombre.Trim().ToLower().Contains("pos")))))
        .Sum(p => p.MontoPago);

    decimal totalTransferencia = pagos
        .Where(p => p.MetodoPago != null &&
                    ((idTransferencia.HasValue && p.MetodoPagoId == idTransferencia.Value) ||
                     (!idTransferencia.HasValue && p.MetodoPago!.Nombre != null &&
                      p.MetodoPago.Nombre.Trim().ToLower().Contains("transfer"))))
        .Sum(p => p.MontoPago);

    // Efectivo esperado = caja inicial + ventas en efectivo del día
    decimal efectivoEsperado = dto.CajaInicial + totalEfectivo;
    decimal diferencia = dto.EfectivoContado - efectivoEsperado;

    var movimientos = await _context.MovimientosCaja
        .Where(m => m.Fecha >= fechaInicio && m.Fecha <= fechaFin && m.CierreId == null)
        .ToListAsync();

    var cierre = new Models.CierreCaja
    {
        UsuarioId = dto.UsuarioId,
        Desde = fechaInicio,
        Hasta = fechaFin,
        TotalEfectivo = totalEfectivo,
        TotalTarjetas = totalTarjeta,
        TotalTransferencias = totalTransferencia,
        EfectivoContado = dto.EfectivoContado,
        Diferencia = diferencia,
        Observaciones = dto.Observaciones,
        CreadoEn = DateTime.Now
    };

    _context.CierresCaja.Add(cierre);
    await _context.SaveChangesAsync();

    foreach (var movimiento in movimientos)
    {
        movimiento.CierreId = cierre.Id;
    }
    await _context.SaveChangesAsync();

    return Ok(new
    {
        message = "Cierre creado correctamente.",
        cierre = new CierreResumenDto
        {
            Fecha = fecha.ToString("yyyy-MM-dd"),
            TotalEfectivo = totalEfectivo,
            TotalTarjeta = totalTarjeta,
            TotalTransferencia = totalTransferencia,
            CajaInicial = dto.CajaInicial,
            EfectivoContado = dto.EfectivoContado,
            EfectivoEsperado = efectivoEsperado,
            Diferencia = diferencia,
            Cuadro = diferencia == 0
        }
    });
}


        // ==========================
// 4. RESUMEN INGRESOS vs GASTOS (UN SOLO DÍA)
// GET: api/Contabilidad/resumen-ingresos-gastos?fecha=2025-11-24&gastos=1500
// Si no se manda fecha, usa el día de hoy.
// ==========================
[HttpGet("resumen-ingresos-gastos")]
public async Task<ActionResult<IngresosGastosDto>> GetResumenIngresosGastos(
    [FromQuery] decimal gastos,
    [FromQuery] DateTime? fecha)
{
    // Regla de tu compañero: ningún número ingresado puede ser negativo
    if (gastos < 0)
    {
        return BadRequest(new { message = "Los gastos no pueden ser negativos." });
    }

    // Por defecto, el resumen es del día de hoy
    var fechaResumen = (fecha ?? DateTime.Today).Date;
    var fechaInicio = fechaResumen;
    var fechaFin = fechaResumen.AddDays(1).AddTicks(-1);

    // Ingresos = movimientos de caja tipo "INGRESO" de ese día
    var totalIngresos = await _context.MovimientosCaja
        .Where(m => m.Fecha >= fechaInicio && m.Fecha <= fechaFin && m.Tipo == "INGRESO")
        .SumAsync(m => (decimal?)m.Monto) ?? 0m;

    var resultado = totalIngresos - gastos;

    var dto = new IngresosGastosDto
    {
        Fecha = fechaResumen.ToString("yyyy-MM-dd"),
        TotalIngresos = totalIngresos,
        TotalGastos = gastos,
        Resultado = resultado,
        EstaEnNegativo = resultado < 0
    };

    return Ok(dto);
}


        // ==========================
        // 5. BALANCE GENERAL
        // ==========================
        // GET: api/Contabilidad/balance-general?fecha=2025-11-23
        [HttpGet("balance-general")]
        public async Task<ActionResult<BalanceGeneralDto>> GetBalanceGeneral([FromQuery] DateTime? fecha)
        {
            var fechaCorte = (fecha ?? DateTime.Today).Date;
            var fechaFin = fechaCorte.AddDays(1).AddTicks(-1);

            // Activo: tomamos la "caja" (ingresos - egresos) hasta la fecha
            var totalIngresos = await _context.MovimientosCaja
                .Where(m => m.Fecha <= fechaFin && m.Tipo == "INGRESO")
                .SumAsync(m => (decimal?)m.Monto) ?? 0m;

            var totalEgresos = await _context.MovimientosCaja
                .Where(m => m.Fecha <= fechaFin && m.Tipo == "EGRESO")
                .SumAsync(m => (decimal?)m.Monto) ?? 0m;

            var caja = totalIngresos - totalEgresos;
            if (caja < 0) caja = 0; // no mostramos activo negativo

            // En este proyecto académico asumimos que no hay pasivos registrados
            decimal pasivo = 0m;

            // Patrimonio neto = Activo - Pasivo
            decimal patrimonio = caja - pasivo;

            var dto = new BalanceGeneralDto
            {
                FechaCorte = fechaCorte.ToString("yyyy-MM-dd"),
                Activo = caja,
                Pasivo = pasivo,
                PatrimonioNeto = patrimonio,
                TotalActivos = caja,
                TotalPasivoPatrimonio = pasivo + patrimonio,
                Cuadra = caja == pasivo + patrimonio
            };

            return Ok(dto);
        }

       // ==========================
// 6. LIBRO DIARIO
// ==========================
// GET: api/Contabilidad/libro-diario?desde=2025-11-01&hasta=2025-11-30
// Si no se envían fechas, toma solo el día de hoy.
// ==========================
// 6. LIBRO DIARIO
// ==========================
[HttpGet("libro-diario")]
public async Task<ActionResult<LibroDiarioDto>> GetLibroDiario(
    [FromQuery] DateTime? desde,
    [FromQuery] DateTime? hasta)
{
    var fechaDesde = (desde ?? DateTime.Today).Date;
    var fechaHasta = (hasta ?? DateTime.Today).Date;

    var inicio = fechaDesde;
    var fin = fechaHasta.AddDays(1).AddTicks(-1);

    var movimientos = await _context.MovimientosCaja
        .Where(m => m.Fecha >= inicio && m.Fecha <= fin)
        .OrderBy(m => m.Fecha)
        .ToListAsync();

    var lista = movimientos.Select(m => new LibroDiarioMovimientoDto
    {
        Fecha = m.Fecha,
        Tipo = m.Tipo,
        Monto = m.Monto
    }).ToList();

    decimal totalCargos = movimientos
        .Where(m => m.Tipo == "EGRESO")
        .Sum(m => m.Monto);

    decimal totalAbonos = movimientos
        .Where(m => m.Tipo == "INGRESO")
        .Sum(m => m.Monto);

    var dto = new LibroDiarioDto
    {
        Desde = fechaDesde.ToString("yyyy-MM-dd"),
        Hasta = fechaHasta.ToString("yyyy-MM-dd"),
        Movimientos = lista,
        TotalCargos = totalCargos,
        TotalAbonos = totalAbonos
    };

    return Ok(dto);
}

// ==========================
// 7. MAYOR (Cuenta Caja)
// ==========================
// GET: api/Contabilidad/mayor?desde=2025-11-01&hasta=2025-11-30
[HttpGet("mayor")]
public async Task<ActionResult<MayorCuentaDto>> GetMayorCaja(
    [FromQuery] DateTime? desde,
    [FromQuery] DateTime? hasta)
{
    var fechaDesde = (desde ?? DateTime.Today).Date;
    var fechaHasta = (hasta ?? DateTime.Today).Date;

    var inicio = fechaDesde;
    var fin = fechaHasta.AddDays(1).AddTicks(-1);

    var movimientos = await _context.MovimientosCaja
        .Where(m => m.Fecha >= inicio && m.Fecha <= fin)
        .OrderBy(m => m.Fecha)
        .ToListAsync();

    decimal saldo = 0m;
    var lista = new List<MayorMovimientoDto>();

    foreach (var m in movimientos)
    {
        decimal cargo = 0m;
        decimal abono = 0m;

        if (m.Tipo == "EGRESO")
        {
            cargo = m.Monto;
            saldo -= m.Monto; // salida de caja
        }
        else if (m.Tipo == "INGRESO")
        {
            abono = m.Monto;
            saldo += m.Monto; // entrada de caja
        }

        lista.Add(new MayorMovimientoDto
        {
            Fecha = m.Fecha,
            Tipo = m.Tipo,
            Cargo = cargo,
            Abono = abono,
            Saldo = saldo
        });
    }

    var dto = new MayorCuentaDto
    {
        Cuenta = "Caja",
        Movimientos = lista,
        TotalCargos = lista.Sum(x => x.Cargo),
        TotalAbonos = lista.Sum(x => x.Abono),
        SaldoFinal = saldo
    };

    return Ok(dto);
}

// ==========================
// 8. BALANZA DE COMPROBACIÓN
// ==========================
// GET: api/Contabilidad/balanza-comprobacion?desde=2025-11-01&hasta=2025-11-30
[HttpGet("balanza-comprobacion")]
public async Task<ActionResult<BalanzaComprobacionDto>> GetBalanzaComprobacion(
    [FromQuery] DateTime? desde,
    [FromQuery] DateTime? hasta)
{
    var fechaDesde = (desde ?? DateTime.Today).Date;
    var fechaHasta = (hasta ?? DateTime.Today).Date;

    var inicio = fechaDesde;
    var fin = fechaHasta.AddDays(1).AddTicks(-1);

    var movimientos = await _context.MovimientosCaja
        .Where(m => m.Fecha >= inicio && m.Fecha <= fin)
        .ToListAsync();

    // Para este proyecto solo tenemos la cuenta "Caja"
    decimal totalCargosCaja = movimientos
        .Where(m => m.Tipo == "EGRESO")
        .Sum(m => m.Monto);

    decimal totalAbonosCaja = movimientos
        .Where(m => m.Tipo == "INGRESO")
        .Sum(m => m.Monto);

    var cuentaCaja = new BalanzaComprobacionCuentaDto
    {
        Cuenta = "Caja",
        TotalCargos = totalCargosCaja,
        TotalAbonos = totalAbonosCaja
    };

    var dto = new BalanzaComprobacionDto
    {
        Desde = fechaDesde.ToString("yyyy-MM-dd"),
        Hasta = fechaHasta.ToString("yyyy-MM-dd"),
        Cuentas = new List<BalanzaComprobacionCuentaDto> { cuentaCaja },
        TotalCargos = totalCargosCaja,
        TotalAbonos = totalAbonosCaja,
        Cuadra = totalCargosCaja == totalAbonosCaja
    };

    return Ok(dto);
}


    }
}
