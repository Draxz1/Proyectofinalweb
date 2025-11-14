using System.ComponentModel.DataAnnotations.Schema;

namespace megadeliciasapi.Models
{
    public class CierreCaja
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public DateTime Desde { get; set; }
        public DateTime Hasta { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal TotalEfectivo { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal TotalTarjetas { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal TotalTransferencias { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal EfectivoContado { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal Diferencia { get; set; }

        public string? Observaciones { get; set; }
        public DateTime CreadoEn { get; set; } = DateTime.Now;

        // --- Relaciones ---
        [ForeignKey("UsuarioId")]
        public Usuario Usuario { get; set; }

        public ICollection<MovimientoCaja> MovimientosIncluidos { get; set; }
    }
}