namespace megadeliciasapi.Models
{
    public class EstadoOrden
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string? Descripcion { get; set; }

        public ICollection<Orden> Ordenes { get; set; }
    }
}