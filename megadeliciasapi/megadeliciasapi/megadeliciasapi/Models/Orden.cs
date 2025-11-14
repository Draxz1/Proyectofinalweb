using System.ComponentModel.DataAnnotations.Schema;

namespace megadeliciasapi.Models
{
    public class Orden
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int MesaId { get; set; }
        public string TipoOrden { get; set; }
        public int EstadoId { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal TotalOrden { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        
        [ForeignKey("UsuarioId")]
        public Usuario Usuario { get; set; }

        [ForeignKey("MesaId")]
        public Mesa Mesa { get; set; }

        [ForeignKey("EstadoId")]
        public EstadoOrden Estado { get; set; }

        public ICollection<DetalleOrden> Detalles { get; set; }
        public Venta Venta { get; set; } 
    }
}