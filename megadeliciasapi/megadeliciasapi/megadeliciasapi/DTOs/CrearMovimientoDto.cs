using System.ComponentModel.DataAnnotations;

namespace megadeliciasapi.DTOs
{
    public class CrearMovimientoDto
    {
        [Required]
        public int UsuarioId { get; set; }

        [Required]
        [MaxLength(20)]
        public string Tipo { get; set; } // "INGRESO" | "EGRESO"

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser positivo")]
        public decimal Monto { get; set; }

        [Required]
        public int MetodoPagoId { get; set; }

        public string? Descripcion { get; set; }

        public int? PagoId { get; set; }
    }
}
