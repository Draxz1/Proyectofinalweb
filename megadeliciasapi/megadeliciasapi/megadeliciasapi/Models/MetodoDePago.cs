using System.Collections.Generic;

namespace megadeliciasapi.Models
{
    public class MetodoPago
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        
        // ✅ Propiedad agregada para solucionar el error
        public bool Activo { get; set; } = true; 

        public ICollection<Pago> Pagos { get; set; }
        public ICollection<MovimientoCaja> MovimientosCaja { get; set; }
    }
}