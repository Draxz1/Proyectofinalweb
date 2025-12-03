 using System.ComponentModel.DataAnnotations;

 namespace megadeliciasapi.DTOs
{
 public class MayorCuentaDto
    {
        // Para este proyecto trabajamos solo con la cuenta "Caja"
        public string Cuenta { get; set; } = string.Empty;
        public List<MayorMovimientoDto> Movimientos { get; set; } = new();
        public decimal TotalCargos { get; set; }
        public decimal TotalAbonos { get; set; }
        public decimal SaldoFinal { get; set; }
    }
    
}