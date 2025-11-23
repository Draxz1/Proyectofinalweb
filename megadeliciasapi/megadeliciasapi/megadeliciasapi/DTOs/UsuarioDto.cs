namespace megadeliciasapi.DTOs
{
    public class UsuarioDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Correo { get; set; }
        public string Rol { get; set; }
        public DateTime CreadoEn { get; set; }
        public bool RequiereCambioPassword { get; set; }
    }
}

