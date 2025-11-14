using System.ComponentModel.DataAnnotations.Schema;

namespace megadeliciasapi.Models
{
    public class Pago
    {
        public int Id { get; set; }
        public int VentaId { get; set; }
        public int MetodoPagoId { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal MontoPago { get; set; }

        public string? Referencia { get; set; }
        public DateTime FechaPago { get; set; } = DateTime.Now;
        public string Estado { get; set; }

        
        [ForeignKey("VentaId")]
        public Venta Venta { get; set; }

        [ForeignKey("MetodoPagoId")]
        public MetodoPago MetodoPago { get; set; }

        public Factura? Factura { get; set; }
        public MovimientoCaja? MovimientoCaja { get; set; }
    }
}