using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations; // (Opcional, por si usas validaciones)
using System.ComponentModel.DataAnnotations.Schema;

namespace megadeliciasapi.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Correo { get; set; }
        public string PasswordHash { get; set; } 
        public string Rol { get; set; }
        public DateTime CreadoEn { get; set; } = DateTime.Now;

        
        public bool RequiereCambioPassword { get; set; } = false; 
        
        // Fecha límite para usar la contraseña temporal 
        public DateTime? PasswordTemporalExpira { get; set; } 

        // --- Relaciones (EF Core) ---
        public ICollection<Orden> Ordenes { get; set; } = new List<Orden>();
        public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
        public ICollection<MovimientoCaja> MovimientosCaja { get; set; } = new List<MovimientoCaja>();
        public ICollection<CierreCaja> CierresCaja { get; set; } = new List<CierreCaja>();
    }
}