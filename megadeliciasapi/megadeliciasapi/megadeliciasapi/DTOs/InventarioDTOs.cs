namespace megadeliciasapi.DTOs
{
    // 1. Para mostrar la tabla de inventario (Imagen derecha)
    public class InventarioItemDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string Categoria { get; set; }
        public int StockActual { get; set; }
        public decimal CostoUnitario { get; set; }
        public decimal ValorTotal => StockActual * CostoUnitario; // Campo calculado
        public string UnidadMedida { get; set; }
    }

    // 2. Para el formulario de registro (Imagen izquierda)
    public class RegistrarMovimientoDto
    {
        public int ItemId { get; set; }
        public string Tipo { get; set; } // "Entrada" o "Salida"
        public int Cantidad { get; set; }
        public decimal CostoUnitario { get; set; } // Opcional (solo para entradas)
        public string Motivo { get; set; }
    }

    // 3. Para responderle al Mesero (Validación)
    public class DisponibilidadPlatoDto
    {
        public int PlatoId { get; set; }
        public bool EstaDisponible { get; set; }
        public List<string> IngredientesFaltantes { get; set; } = new List<string>();
    }

    public class MovimientoDto
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string ItemNombre { get; set; }
        public string Tipo { get; set; }
        public int Cantidad { get; set; }
        public decimal CostoUnitario { get; set; }
        public string Motivo { get; set; }
    }

    public class CrearItemDto
    {
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public int CategoriaId { get; set; }
        public string UnidadMedida { get; set; } 
        public int StockMinimo { get; set; }
    }
}