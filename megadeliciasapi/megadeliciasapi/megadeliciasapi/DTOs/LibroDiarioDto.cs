using System.ComponentModel.DataAnnotations;

namespace  megadeliciasapi.DTOs {
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