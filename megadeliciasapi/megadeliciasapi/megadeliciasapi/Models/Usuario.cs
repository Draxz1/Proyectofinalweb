using System;
using System.Collections.Generic;

namespace megadeliciasapi.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Correo { get; set; }
        public string PasswordHash { get; set; } // Lo necesitamos para el login
        public string Rol { get; set; }
        public DateTime CreadoEn { get; set; } = DateTime.Now;

        // --- Relaciones (para EF Core) ---
        public ICollection<Orden> Ordenes { get; set; }
        public ICollection<Venta> Ventas { get; set; }
        public ICollection<MovimientoCaja> MovimientosCaja { get; set; }
        public ICollection<CierreCaja> CierresCaja { get; set; }
    }
}