using System.ComponentModel.DataAnnotations;

namespace megadeliciasapi.DTOs
{
    public class CambiarPasswordUsuarioDto
    {
        [Required(ErrorMessage = "La nueva contraseña es requerida")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string NuevaPassword { get; set; }
    }
}

