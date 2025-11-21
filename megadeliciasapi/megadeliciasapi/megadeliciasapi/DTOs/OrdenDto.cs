namespace megadeliciasapi.DTOs
{
    // Lo que envía el frontend para crear una orden
    public class CrearOrdenDto
    {
        public int? MesaId { get; set; } // Opcional
        public List<CrearOrdenDetalleDto> Detalles { get; set; }
    }

    public class CrearOrdenDetalleDto
    {
        public int PlatoId { get; set; }
        public int Cantidad { get; set; }
        public string? Nota { get; set; }
    }
    
    // Para cambiar el estado (Cocina)
    public class ActualizarEstadoDto
    {
        public string Estado { get; set; }
    }
}