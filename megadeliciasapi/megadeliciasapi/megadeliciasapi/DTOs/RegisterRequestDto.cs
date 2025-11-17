namespace megadeliciasapi.DTOs
{
    public class RegisterRequestDto
    {
        public string Nombre { get; set; }
        public string Correo { get; set; }
        public string Password { get; set; }
        public string Rol { get; set; }
    }
}