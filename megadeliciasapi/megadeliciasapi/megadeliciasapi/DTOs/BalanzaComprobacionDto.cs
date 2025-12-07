using System.ComponentModel.DataAnnotations;

namespace megadeliciasapi.DTOs 
{
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
}