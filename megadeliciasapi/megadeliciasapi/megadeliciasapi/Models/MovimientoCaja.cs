using System.ComponentModel.DataAnnotations.Schema;

namespace megadeliciasapi.Models
{
    public class MovimientoCaja
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string Tipo { get; set; } // "INGRESO", "EGRESO"

        [Column(TypeName = "decimal(10, 2)")]
        public decimal Monto { get; set; }

        public int MetodoPagoId { get; set; }
        public string? Descripcion { get; set; }
        public int? PagoId { get; set; } 
        public DateTime Fecha { get; set; } = DateTime.Now;
        public int? CierreId { get; set; } 

        // --- Relaciones ---
        [ForeignKey("UsuarioId")]
        public Usuario Usuario { get; set; }

        [ForeignKey("MetodoPagoId")]
        public MetodoPago MetodoPago { get; set; }

        [ForeignKey("PagoId")]
        public Pago? Pago { get; set; }

        [ForeignKey("CierreId")]
        public CierreCaja? CierreCaja { get; set; }
    }
}