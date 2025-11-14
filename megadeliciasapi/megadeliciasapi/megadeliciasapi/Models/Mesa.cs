namespace megadeliciasapi.Models
{
    public class Mesa
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public int Capacidad { get; set; }
        public bool Activa { get; set; }

        public ICollection<Orden> Ordenes { get; set; }
    }
}