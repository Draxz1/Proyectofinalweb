namespace megadeliciasapi.DTOs
{
    public class LibroDiarioMovimientoDto
    {
        public DateTime Fecha { get; set; }
        public string Tipo { get; set; } = string.Empty;   // "INGRESO" o "EGRESO"
        public decimal Monto { get; set; }
    }
}