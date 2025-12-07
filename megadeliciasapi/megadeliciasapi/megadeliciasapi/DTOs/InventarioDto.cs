using System.ComponentModel.DataAnnotations;

namespace megadeliciasapi.DTOs
{
    // DTO para listar productos
    public class ProductoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public decimal PrecioUnitario { get; set; }
        public int Stock { get; set; }
        public int StockMinimo { get; set; }
        public string? UnidadMedida { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
    }

    // DTO para crear producto
    public class CrearProductoDto
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "La categoría es requerida")]
        [StringLength(50, ErrorMessage = "La categoría no puede exceder 50 caracteres")]
        public string Categoria { get; set; } = string.Empty;

        [Required(ErrorMessage = "El precio unitario es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
        public decimal PrecioUnitario { get; set; }

        [Required(ErrorMessage = "El stock es requerido")]
        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo")]
        public int Stock { get; set; }

        [Required(ErrorMessage = "El stock mínimo es requerido")]
        [Range(0, int.MaxValue, ErrorMessage = "El stock mínimo no puede ser negativo")]
        public int StockMinimo { get; set; }

        [StringLength(20, ErrorMessage = "La unidad de medida no puede exceder 20 caracteres")]
        public string? UnidadMedida { get; set; }

        public bool Activo { get; set; } = true;
    }

    // DTO para actualizar producto
    public class ActualizarProductoDto
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "La categoría es requerida")]
        [StringLength(50, ErrorMessage = "La categoría no puede exceder 50 caracteres")]
        public string Categoria { get; set; } = string.Empty;

        [Required(ErrorMessage = "El precio unitario es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
        public decimal PrecioUnitario { get; set; }

        [Required(ErrorMessage = "El stock es requerido")]
        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo")]
        public int Stock { get; set; }

        [Required(ErrorMessage = "El stock mínimo es requerido")]
        [Range(0, int.MaxValue, ErrorMessage = "El stock mínimo no puede ser negativo")]
        public int StockMinimo { get; set; }

        [StringLength(20, ErrorMessage = "La unidad de medida no puede exceder 20 caracteres")]
        public string? UnidadMedida { get; set; }

        public bool Activo { get; set; }
    }

    // DTO para actualizar solo el stock
    public class ActualizarStockDto
    {
        [Required(ErrorMessage = "La cantidad es requerida")]
        public int Cantidad { get; set; }

        [Required(ErrorMessage = "El tipo de operación es requerido")]
        [RegularExpression("^(ENTRADA|SALIDA)$", ErrorMessage = "El tipo debe ser ENTRADA o SALIDA")]
        public string Tipo { get; set; } = string.Empty; // ENTRADA o SALIDA

        [StringLength(200, ErrorMessage = "La razón no puede exceder 200 caracteres")]
        public string? Razon { get; set; }
    }

    // DTO para respuestas
    public class InventarioResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }
    }
}