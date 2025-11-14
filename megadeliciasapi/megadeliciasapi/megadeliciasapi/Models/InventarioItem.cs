using System.ComponentModel.DataAnnotations.Schema;

namespace megadeliciasapi.Models
{
    public class InventarioItem
    {
        public int Id { get; set; }
        public string? Codigo { get; set; }
        public string Nombre { get; set; }
        public int StockActual { get; set; }
        public int StockMinimo { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal CostoUnitario { get; set; }

        public int CategoriaId { get; set; }
        public string UnidadMedida { get; set; }
        public bool Activo { get; set; }
        public DateTime CreadoEn { get; set; } = DateTime.Now;

        // --- Relaciones ---
        [ForeignKey("CategoriaId")]
        public Categoria Categoria { get; set; }

        public ICollection<PlatoIngrediente> RecetasDondeSeUsa { get; set; }
        public ICollection<InventarioMovimiento> Movimientos { get; set; }
    }
}