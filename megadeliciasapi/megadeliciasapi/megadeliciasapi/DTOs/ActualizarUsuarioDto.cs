using System.ComponentModel.DataAnnotations;

namespace megadeliciasapi.DTOs
{
    public class ActualizarUsuarioDto
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El correo es requerido")]
        [EmailAddress(ErrorMessage = "El correo no es válido")]
        public string Correo { get; set; }

        [Required(ErrorMessage = "El rol es requerido")]
        public string Rol { get; set; }
    }
}

