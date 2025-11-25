using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace megadeliciasapi.DTOs
{
    public class CrearInventarioDTOs
    {

        [Required(ErrorMessage = "El código es obligatorio.")]
        public string? Codigo { get; set; }
        
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [MaxLength(100)]
        public string Nombre { get; set; }
        
        public int StockActual { get; set; } = 0; 
        
        [Required(ErrorMessage = "El stock mínimo es obligatorio.")]
        [Range(0, int.MaxValue)]
        public int StockMinimo { get; set; }

        [Column(TypeName = "decimal(10, 2)")] 
        [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "El costo debe ser positivo.")]
        public decimal CostoUnitario { get; set; }

        [Required(ErrorMessage = "La categoría es obligatoria.")]
        public int CategoriaId { get; set; }
        
        [Required(ErrorMessage = "La unidad de medida es obligatoria.")]
        public string UnidadMedida { get; set; }
        
        public bool Activo { get; set; } = true;
        
    }
}