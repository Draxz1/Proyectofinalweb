using System.ComponentModel.DataAnnotations;

namespace megadeliciasapi.DTOs
{
    // ==========================
    // 1. CIERRE DIARIO / POR FECHA
    // ==========================
    // ===== CIERRE CAJA =====
    public class CierreResumenDto
    {
        public string Fecha { get; set; } = string.Empty;

        public decimal TotalEfectivo { get; set; }
        public decimal TotalTarjeta { get; set; }
        public decimal TotalTransferencia { get; set; }

        public decimal CajaInicial { get; set; }
        public decimal EfectivoEsperado { get; set; }
        public decimal EfectivoContado { get; set; }
        public decimal Diferencia { get; set; }
        public bool Cuadro { get; set; }
    }

    public class CrearCierreContabilidadDto
    {
        [Required]
        public int UsuarioId { get; set; }

        [Required]
        public DateTime Fecha { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "La caja inicial no puede ser negativa.")]
        public decimal CajaInicial { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "El efectivo contado no puede ser negativo.")]
        public decimal EfectivoContado { get; set; }

        public string? Observaciones { get; set; }
    }

    // ===== INGRESOS vs GASTOS =====
    public class IngresosGastosDto
{
    public string Fecha { get; set; } = string.Empty;

    public decimal TotalIngresos { get; set; }
    public decimal TotalGastos { get; set; }
    public decimal Resultado { get; set; }
    public bool EstaEnNegativo { get; set; }
}


    // ===== BALANCE GENERAL =====
    public class BalanceGeneralDto
    {
        public string FechaCorte { get; set; } = string.Empty;

        // Activo total (por simplicidad: caja del negocio)
        public decimal Activo { get; set; }

        // Pasivo total (en este proyecto lo dejamos 0,
        // pero lo tienes separado para mostrar el concepto)
        public decimal Pasivo { get; set; }

        // Patrimonio neto = Activo - Pasivo
        public decimal PatrimonioNeto { get; set; }

        public decimal TotalActivos { get; set; }
        public decimal TotalPasivoPatrimonio { get; set; }

        // True si se cumple A = P + PN
        public bool Cuadra { get; set; }
    }

public class LibroDiarioMovimientoDto
    {
        public DateTime Fecha { get; set; }
        // INGRESO o EGRESO (según MovimientosCaja.Tipo)
        public string Tipo { get; set; } = string.Empty;
        public decimal Monto { get; set; }
    }

   
    // ==========================
    // MAYOR (para cuenta Caja)
    // ==========================
    public class MayorMovimientoDto
    {
        public DateTime Fecha { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public decimal Cargo { get; set; }   // egreso
        public decimal Abono { get; set; }   // ingreso
        public decimal Saldo { get; set; }   // saldo acumulado
    }


    public class MayorCuentaDto
    {
        public string Cuenta { get; set; } = string.Empty;
        public List<MayorMovimientoDto> Movimientos { get; set; } = new();
        public decimal TotalCargos { get; set; }
        public decimal TotalAbonos { get; set; }
        public decimal SaldoFinal { get; set; }
    }

    // ==========================
    // 6. BALANZA DE COMPROBACIÓN
    // ==========================
    public class BalanzaComprobacionCuentaDto
    {
        public string Cuenta { get; set; } = string.Empty;
        public decimal TotalCargos { get; set; }
        public decimal TotalAbonos { get; set; }
    }

    public class BalanzaComprobacionDto
    {
        public string Desde { get; set; } = string.Empty;
        public string Hasta { get; set; } = string.Empty;

        public List<BalanzaComprobacionCuentaDto> Cuentas { get; set; } = new();

        public decimal TotalCargos { get; set; }
        public decimal TotalAbonos { get; set; }
        public bool Cuadra { get; set; }  // true si TotalCargos == TotalAbonos
    }

     public class LibroDiarioDto
    {
        public string Desde { get; set; } = string.Empty;
        public string Hasta { get; set; } = string.Empty;

        public List<LibroDiarioMovimientoDto> Movimientos { get; set; } = new();

        // En contabilidad: cargos = egresos, abonos = ingresos
        public decimal TotalCargos { get; set; } // EGRESO
        public decimal TotalAbonos { get; set; } // INGRESO
    }
}
