namespace megadeliciasapi.Models
{
    public class MetodoPago
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public ICollection<Pago> Pagos { get; set; }
        public ICollection<MovimientoCaja> MovimientosCaja { get; set; }
    }
}