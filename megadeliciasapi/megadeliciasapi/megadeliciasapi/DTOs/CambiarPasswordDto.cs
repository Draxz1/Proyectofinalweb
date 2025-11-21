namespace megadeliciasapi.DTOs
{
    public class CambiarPasswordDto
    {
        public string Correo { get; set; }
        public string PasswordTemporal { get; set; }
        public string NuevoPassword { get; set; }
    }
}