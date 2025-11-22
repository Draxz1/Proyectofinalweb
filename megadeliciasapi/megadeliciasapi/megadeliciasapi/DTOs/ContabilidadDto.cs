  public class ResumenContableDto
    {
        public string FechaInicio { get; set; } = string.Empty;
        public string FechaFin { get; set; } = string.Empty;
        public decimal TotalVentas { get; set; }
        public decimal TotalFacturado { get; set; }
        public decimal TotalImpuesto { get; set; }
        public string Comentario { get; set; } = string.Empty;
    }

    public class CierreContableSolicitudDto
    {
        public DateTime FechaCierre { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public string Observaciones { get; set; } = string.Empty;
    }

    public class CierreContableRespuestaDto
    {
        public string FechaCierre { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public decimal TotalVentasCerradas { get; set; }
        public string Mensaje { get; set; } = string.Empty;
    }
