namespace megadeliciasapi.DTOs
{
    public class MayorMovimientoDto
    {
        public DateTime Fecha { get; set; }
        public string Descripcion { get; set; } = string.Empty;

        // Valores originales
        public decimal Cargo { get; set; }
        public decimal Abono { get; set; }
        public decimal Saldo { get; set; }

        // Valor requerido por el Controller
        public string Tipo { get; set; } = string.Empty; // "Cargo" o "Abono"
    }
}
