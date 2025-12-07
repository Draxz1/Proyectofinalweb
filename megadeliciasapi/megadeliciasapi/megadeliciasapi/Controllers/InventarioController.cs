using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using megadeliciasapi.Data;
using megadeliciasapi.Models;
using megadeliciasapi.DTOs;

namespace megadeliciasapi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InventarioController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<InventarioController> _logger;

        public InventarioController(ApplicationDbContext context, ILogger<InventarioController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("productos")]
        public async Task<ActionResult<IEnumerable<ProductoDto>>> GetProductos(
            [FromQuery] bool? soloActivos = null,
            [FromQuery] string? categoria = null,
            [FromQuery] bool? stockBajo = null)
        {
            try
            {
                var query = _context.Productos.AsQueryable();

                if (soloActivos == true)
                {
                    query = query.Where(p => p.Activo);
                }

                if (!string.IsNullOrEmpty(categoria))
                {
                    query = query.Where(p => p.Categoria == categoria);
                }

                if (stockBajo == true)
                {
                    query = query.Where(p => p.Stock <= p.StockMinimo);
                }

                var productos = await query
                    .OrderBy(p => p.Categoria)
                    .ThenBy(p => p.Nombre)
                    .ToListAsync();

                var productosDto = productos.Select(p => new ProductoDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    Categoria = p.Categoria,
                    PrecioUnitario = p.PrecioUnitario,
                    Stock = p.Stock,
                    StockMinimo = p.StockMinimo,
                    UnidadMedida = p.UnidadMedida,
                    Activo = p.Activo,
                    FechaCreacion = p.FechaCreacion,
                    FechaActualizacion = p.FechaActualizacion
                }).ToList();

                return Ok(productosDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos");
                return StatusCode(500, new { message = "Error al obtener productos" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductoDto>> GetProducto(int id)
        {
            try
            {
                var producto = await _context.Productos.FindAsync(id);

                if (producto == null)
                {
                    return NotFound(new { message = "Producto no encontrado" });
                }

                var productoDto = new ProductoDto
                {
                    Id = producto.Id,
                    Nombre = producto.Nombre,
                    Descripcion = producto.Descripcion,
                    Categoria = producto.Categoria,
                    PrecioUnitario = producto.PrecioUnitario,
                    Stock = producto.Stock,
                    StockMinimo = producto.StockMinimo,
                    UnidadMedida = producto.UnidadMedida,
                    Activo = producto.Activo,
                    FechaCreacion = producto.FechaCreacion,
                    FechaActualizacion = producto.FechaActualizacion
                };

                return Ok(productoDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener producto {Id}", id);
                return StatusCode(500, new { message = "Error al obtener producto" });
            }
        }

        [HttpPost("crear-producto")]
        public async Task<ActionResult> CrearProducto([FromBody] CrearProductoDto dto)
        {
            try
            {
                var existe = await _context.Productos
                    .AnyAsync(p => p.Nombre.ToLower() == dto.Nombre.ToLower());

                if (existe)
                {
                    return BadRequest(new { message = "Ya existe un producto con ese nombre" });
                }

                var producto = new Producto
                {
                    Nombre = dto.Nombre,
                    Descripcion = dto.Descripcion,
                    Categoria = dto.Categoria,
                    PrecioUnitario = dto.PrecioUnitario,
                    Stock = dto.Stock,
                    StockMinimo = dto.StockMinimo,
                    UnidadMedida = dto.UnidadMedida ?? "Unidad",
                    Activo = true,
                    FechaCreacion = DateTime.Now
                };

                _context.Productos.Add(producto);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Producto creado exitosamente", productoId = producto.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear producto");
                return StatusCode(500, new { message = "Error al crear producto" });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> ActualizarProducto(int id, [FromBody] ActualizarProductoDto dto)
        {
            try
            {
                var producto = await _context.Productos.FindAsync(id);

                if (producto == null)
                {
                    return NotFound(new { message = "Producto no encontrado" });
                }

                var nombreDuplicado = await _context.Productos
                    .AnyAsync(p => p.Nombre.ToLower() == dto.Nombre.ToLower() && p.Id != id);

                if (nombreDuplicado)
                {
                    return BadRequest(new { message = "Ya existe otro producto con ese nombre" });
                }

                producto.Nombre = dto.Nombre;
                producto.Descripcion = dto.Descripcion;
                producto.Categoria = dto.Categoria;
                producto.PrecioUnitario = dto.PrecioUnitario;
                producto.Stock = dto.Stock;
                producto.StockMinimo = dto.StockMinimo;
                producto.UnidadMedida = dto.UnidadMedida ?? "Unidad";
                producto.Activo = dto.Activo;
                producto.FechaActualizacion = DateTime.Now;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Producto actualizado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar producto {Id}", id);
                return StatusCode(500, new { message = "Error al actualizar producto" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> EliminarProducto(int id)
        {
            try
            {
                var producto = await _context.Productos.FindAsync(id);

                if (producto == null)
                {
                    return NotFound(new { message = "Producto no encontrado" });
                }

                producto.Activo = false;
                producto.FechaActualizacion = DateTime.Now;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Producto desactivado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar producto {Id}", id);
                return StatusCode(500, new { message = "Error al eliminar producto" });
            }
        }

        [HttpPut("{id}/stock")]
        public async Task<ActionResult> ActualizarStock(int id, [FromBody] ActualizarStockDto dto)
        {
            try
            {
                var producto = await _context.Productos.FindAsync(id);

                if (producto == null)
                {
                    return NotFound(new { message = "Producto no encontrado" });
                }

                if (dto.Tipo.ToUpper() == "SALIDA" && producto.Stock < dto.Cantidad)
                {
                    return BadRequest(new { message = "Stock insuficiente para realizar la salida" });
                }

                if (dto.Tipo.ToUpper() == "ENTRADA")
                {
                    producto.Stock += dto.Cantidad;
                }
                else if (dto.Tipo.ToUpper() == "SALIDA")
                {
                    producto.Stock -= dto.Cantidad;
                }

                producto.FechaActualizacion = DateTime.Now;
                await _context.SaveChangesAsync();

                return Ok(new { 
                    message = "Stock actualizado exitosamente",
                    nuevoStock = producto.Stock
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar stock del producto {Id}", id);
                return StatusCode(500, new { message = "Error al actualizar stock" });
            }
        }

        [HttpGet("categorias")]
        public async Task<ActionResult<IEnumerable<string>>> GetCategorias()
        {
            try
            {
                var categorias = await _context.Productos
                    .Where(p => p.Activo)
                    .Select(p => p.Categoria)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToListAsync();

                return Ok(categorias);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener categorías");
                return StatusCode(500, new { message = "Error al obtener categorías" });
            }
        }

        [HttpGet("stock-bajo")]
        public async Task<ActionResult<IEnumerable<ProductoDto>>> GetProductosStockBajo()
        {
            try
            {
                var productos = await _context.Productos
                    .Where(p => p.Activo && p.Stock <= p.StockMinimo)
                    .OrderBy(p => p.Stock)
                    .ToListAsync();

                var productosDto = productos.Select(p => new ProductoDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    Categoria = p.Categoria,
                    PrecioUnitario = p.PrecioUnitario,
                    Stock = p.Stock,
                    StockMinimo = p.StockMinimo,
                    UnidadMedida = p.UnidadMedida,
                    Activo = p.Activo,
                    FechaCreacion = p.FechaCreacion,
                    FechaActualizacion = p.FechaActualizacion
                }).ToList();

                return Ok(productosDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos con stock bajo");
                return StatusCode(500, new { message = "Error al obtener productos con stock bajo" });
            }
        }

        [HttpGet("movimientos-recientes")]
        public async Task<ActionResult<IEnumerable<MovimientoDto>>> GetMovimientosRecientes(
            [FromQuery] int cantidad = 20)
        {
            try
            {
                var movimientos = new List<MovimientoDto>();
                return Ok(movimientos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener movimientos recientes");
                return StatusCode(500, new { message = "Error al obtener movimientos" });
            }
        }

        [HttpPost("registrar-movimiento")]
        public async Task<ActionResult> RegistrarMovimiento([FromBody] RegistrarMovimientoDto dto)
        {
            try
            {
                var producto = await _context.Productos.FindAsync(dto.ProductoId);

                if (producto == null)
                {
                    return NotFound(new { message = "Producto no encontrado" });
                }

                if (dto.Cantidad <= 0)
                {
                    return BadRequest(new { message = "La cantidad debe ser mayor a 0" });
                }

                if (dto.Tipo.ToUpper() == "SALIDA" && producto.Stock < dto.Cantidad)
                {
                    return BadRequest(new { 
                        message = $"Stock insuficiente. Disponible: {producto.Stock}, Solicitado: {dto.Cantidad}" 
                    });
                }

                if (dto.Tipo.ToUpper() == "ENTRADA" && (!dto.CostoUnitario.HasValue || dto.CostoUnitario <= 0))
                {
                    return BadRequest(new { message = "El costo unitario es requerido para entradas" });
                }

                if (dto.Tipo.ToUpper() == "ENTRADA")
                {
                    producto.Stock += dto.Cantidad;
                    if (dto.CostoUnitario.HasValue)
                    {
                        producto.PrecioUnitario = dto.CostoUnitario.Value;
                    }
                }
                else if (dto.Tipo.ToUpper() == "SALIDA")
                {
                    producto.Stock -= dto.Cantidad;
                }

                producto.FechaActualizacion = DateTime.Now;
                await _context.SaveChangesAsync();

                return Ok(new { 
                    message = "Movimiento registrado exitosamente",
                    nuevoStock = producto.Stock
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar movimiento");
                return StatusCode(500, new { message = "Error al registrar movimiento" });
            }
        }

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

    public class CrearProductoDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public decimal PrecioUnitario { get; set; }
        public int Stock { get; set; }
        public int StockMinimo { get; set; }
        public string? UnidadMedida { get; set; }
        public string? Codigo { get; set; }
    }

    public class ActualizarProductoDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public decimal PrecioUnitario { get; set; }
        public int Stock { get; set; }
        public int StockMinimo { get; set; }
        public string? UnidadMedida { get; set; }
        public bool Activo { get; set; }
    }

    public class ActualizarStockDto
    {
        public int Cantidad { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string? Razon { get; set; }
    }

    public class MovimientoDto
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string ProductoNombre { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal CostoUnitario { get; set; }
        public string Motivo { get; set; } = string.Empty;
    }

    public class RegistrarMovimientoDto
    {
        public int ProductoId { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal? CostoUnitario { get; set; }
        public string? Motivo { get; set; }
        public int? UsuarioId { get; set; }
    }
    }
}