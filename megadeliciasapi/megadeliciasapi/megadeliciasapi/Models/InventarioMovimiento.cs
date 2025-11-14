using System.ComponentModel.DataAnnotations.Schema;

namespace megadeliciasapi.Models
{
    public class InventarioMovimiento
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public int? PlatoIngredienteId { get; set; } 
        public string Tipo { get; set; }
        public int Cantidad { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal CostoUnitario { get; set; }

        public string? Motivo { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;

        // --- Relaciones ---
        [ForeignKey("ItemId")]
        public InventarioItem InventarioItem { get; set; }

        [ForeignKey("PlatoIngredienteId")]
        public PlatoIngrediente? PlatoIngrediente { get; set; }
    }
}