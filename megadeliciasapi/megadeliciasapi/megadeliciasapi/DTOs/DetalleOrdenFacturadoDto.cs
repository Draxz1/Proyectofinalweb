public class DetalleOrdenFacturaDto
{
    public string PlatoNombre { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
    public string? NotaPlato { get; set; }
}