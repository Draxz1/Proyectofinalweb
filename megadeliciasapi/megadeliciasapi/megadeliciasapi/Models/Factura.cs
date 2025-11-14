using System.ComponentModel.DataAnnotations.Schema;

namespace megadeliciasapi.Models
{
    public class Factura
    {
        public int Id { get; set; }
        public int PagoId { get; set; }
        public string NumeroFactura { get; set; }
        public DateTime FechaEmision { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(10, 2)")]
        public decimal Subtotal { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal Impuesto { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal Total { get; set; }

        public string Cai { get; set; }
        public string RangoAutorizado { get; set; }
        public DateTime FechaLimiteEmision { get; set; }
        public string Estado { get; set; }

        // --- Relaciones ---
        [ForeignKey("PagoId")]
        public Pago Pago { get; set; }
    }
}