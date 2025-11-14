using System.ComponentModel.DataAnnotations.Schema;

namespace megadeliciasapi.Models
{
    public class PlatoIngrediente
    {
        public int Id { get; set; }
        public int PlatoId { get; set; }
        public int ItemId { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal CantidadUsada { get; set; }

        public string UnidadMedida { get; set; }
        public DateTime CreadoEn { get; set; } = DateTime.Now;

        // --- Relaciones ---
        [ForeignKey("PlatoId")]
        public Plato Plato { get; set; }

        [ForeignKey("ItemId")]
        public InventarioItem InventarioItem { get; set; }

        public ICollection<InventarioMovimiento> Movimientos { get; set; }
    }
}