namespace megadeliciasapi.DTOs
{
    public class LibroDiarioMovimientoDto
    {
        public DateTime Fecha { get; set; }
        public string Cuenta { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;

        // Valores originales del DTO
        public decimal Debe { get; set; }
        public decimal Haber { get; set; }

        public int? AsientoId { get; set; }

        // Valores requeridos por el ContabilidadController
        public string Tipo { get; set; } = string.Empty;   // "Debe" o "Haber"
        public decimal Monto { get; set; }                 // Valor asociado
    }
}
