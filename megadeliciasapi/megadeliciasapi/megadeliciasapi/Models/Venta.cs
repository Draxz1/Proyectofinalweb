using System.ComponentModel.DataAnnotations.Schema;

namespace megadeliciasapi.Models
{
    public class Venta
    {
        public int Id { get; set; }
        public int OrdenId { get; set; }
        public int UsuarioId { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal TotalVenta { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        
        [ForeignKey("OrdenId")]
        public Orden Orden { get; set; }

        [ForeignKey("UsuarioId")]
        public Usuario Usuario { get; set; }

        public ICollection<Pago> Pagos { get; set; }
    }
}