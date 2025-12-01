namespace megadeliciasapi.DTOs
{
    public class MovimientoResponseDto
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string? UsuarioNombre { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public int MetodoPagoId { get; set; }
        public string? MetodoPagoNombre { get; set; }
        public string? Descripcion { get; set; }
        public int? PagoId { get; set; }
        public DateTime Fecha { get; set; }
        public int? CierreId { get; set; }
    }
}

