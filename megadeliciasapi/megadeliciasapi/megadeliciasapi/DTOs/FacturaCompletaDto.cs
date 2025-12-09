public class FacturaCompletaDto
{
    public int Id { get; set; }
    public int PagoId { get; set; }
    public string NumeroFactura { get; set; }
    public DateTime FechaEmision { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Impuesto { get; set; }
    public decimal Total { get; set; }
    public string Cai { get; set; }
    public string RangoAutorizado { get; set; }
    public DateTime FechaLimiteEmision { get; set; }
    public string Estado { get; set; }
    public string Mesero { get; set; }
    public string Mesa { get; set; }
    public List<DetalleOrdenFacturaDto> Detalles { get; set; }
}