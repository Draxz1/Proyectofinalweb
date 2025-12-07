namespace megadeliciasapi.DTOs
{
    public class MayorMovimientoDto
    {
        public DateTime Fecha { get; set; }
        public string Tipo { get; set; } = string.Empty;  // "INGRESO" o "EGRESO"
        public decimal Cargo { get; set; }   // Si es EGRESO
        public decimal Abono { get; set; }   // Si es INGRESO
        public decimal Saldo { get; set; }   // Saldo acumulado
    }
}