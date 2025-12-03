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

        public decimal Activo { get; set; }
        public decimal Pasivo { get; set; }
        public decimal PatrimonioNeto { get; set; }

        public decimal TotalActivos { get; set; }
        public decimal TotalPasivoPatrimonio { get; set; }

        public bool Cuadra { get; set; }
    }

    // NOTA: Se eliminaron LibroDiarioMovimientoDto, MayorMovimientoDto, 
    // BalanzaComprobacionDto, etc. porque ya tienen sus propios archivos .cs
}