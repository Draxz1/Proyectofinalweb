using System.ComponentModel.DataAnnotations.Schema;

namespace megadeliciasapi.Models
{
    public class DetalleOrden
    {
        public int Id { get; set; }
        public int OrdenId { get; set; }
        public int PlatoId { get; set; }
        public int Cantidad { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal PrecioUnitario { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal Subtotal { get; set; }

        public string? NotaPlato { get; set; }
        public DateTime CreadoEn { get; set; } = DateTime.Now;

        // --- Relaciones ---
        [ForeignKey("OrdenId")]
        public Orden Orden { get; set; }

        [ForeignKey("PlatoId")]
        public Plato Plato { get; set; }
    }
}