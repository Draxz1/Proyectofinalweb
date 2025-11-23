using System;
using System.ComponentModel.DataAnnotations;

namespace megadeliciasapi.DTOs
{
    public class CrearCierreDto
    {
        [Required]
        public int UsuarioId { get; set; } // quien realiza el cierre

        [Required]
        public DateTime Desde { get; set; }

        [Required]
        public DateTime Hasta { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal EfectivoContado { get; set; }

        public string? Observaciones { get; set; }
    }
}
