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

        // GET: api/Inventario/productos
        [HttpGet("productos")]
        public async Task<ActionResult<IEnumerable<ProductoDto>>> GetProductos(
            [FromQuery] bool? soloActivos = null,
            [FromQuery] string? categoria = null,
            [FromQuery] bool? stockBajo = null)
        {
            try
            {
                // Usamos InventarioItems que es la tabla real vinculada a Cocina
                var query = _context.InventarioItems.Include(i => i.Categoria).AsQueryable();

                if (soloActivos == true) 
                    query = query.Where(p => p.Activo);

                if (!string.IsNullOrEmpty(categoria)) 
                    query = query.Where(p => p.Categoria.Nombre == categoria);

                if (stockBajo == true) 
                    query = query.Where(p => p.StockActual <= p.StockMinimo);

                var items = await query.OrderBy(p => p.Nombre).ToListAsync();

                // Mapeo a ProductoDto para compatibilidad con el frontend
                var productosDto = items.Select(p => new ProductoDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Descripcion = p.Nombre, // InventarioItem no tiene descripción, usamos nombre
                    Categoria = p.Categoria != null ? p.Categoria.Nombre : "Sin Categoría",
                    PrecioUnitario = p.CostoUnitario,
                    Stock = p.StockActual,
                    StockMinimo = p.StockMinimo,
                    UnidadMedida = p.UnidadMedida,
                    Activo = p.Activo,
                    FechaCreacion = p.CreadoEn,
                    FechaActualizacion = DateTime.Now
                }).ToList();

                return Ok(productosDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos");
                return StatusCode(500, new { message = "Error al obtener productos" });
            }
        }

        // GET: api/Inventario/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductoDto>> GetProducto(int id)
        {
            try
            {
                var p = await _context.InventarioItems
                    .Include(i => i.Categoria)
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (p == null) return NotFound(new { message = "Producto no encontrado" });

                var dto = new ProductoDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Descripcion = p.Nombre,
                    Categoria = p.Categoria != null ? p.Categoria.Nombre : "Sin Categoría",
                    PrecioUnitario = p.CostoUnitario,
                    Stock = p.StockActual,
                    StockMinimo = p.StockMinimo,
                    UnidadMedida = p.UnidadMedida,
                    Activo = p.Activo,
                    FechaCreacion = p.CreadoEn,
                    FechaActualizacion = DateTime.Now
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener producto {Id}", id);
                return StatusCode(500, new { message = "Error interno" });
            }
        }

        // POST: api/Inventario/crear-producto
        [HttpPost("crear-producto")]
        public async Task<ActionResult> CrearProducto([FromBody] CrearProductoDto dto)
        {
            try
            {
                // Validación de duplicados
                var existe = await _context.InventarioItems
                    .AnyAsync(p => p.Nombre.ToLower() == dto.Nombre.ToLower());

                if (existe)
                    return BadRequest(new { message = "Ya existe un producto con ese nombre" });

                // Gestionar categoría (buscar o crear)
                var categoria = await _context.Categorias
                    .FirstOrDefaultAsync(c => c.Nombre.ToLower() == dto.Categoria.ToLower());

                if (categoria == null)
                {
                    // CORRECCIÓN: Eliminada la propiedad 'Descripcion' que causaba error
                    categoria = new Categoria { Nombre = dto.Categoria };
                    _context.Categorias.Add(categoria);
                    await _context.SaveChangesAsync();
                }

                var item = new InventarioItem
                {
                    Nombre = dto.Nombre,
                    Codigo = dto.Codigo,
                    CategoriaId = categoria.Id,
                    StockActual = dto.Stock,
                    StockMinimo = dto.StockMinimo,
                    CostoUnitario = dto.PrecioUnitario,
                    UnidadMedida = dto.UnidadMedida ?? "Unidad",
                    Activo = true,
                    CreadoEn = DateTime.Now
                };

                _context.InventarioItems.Add(item);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Producto creado exitosamente", productoId = item.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear producto");
                return StatusCode(500, new { message = "Error al crear producto" });
            }
        }

        // PUT: api/Inventario/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult> ActualizarProducto(int id, [FromBody] ActualizarProductoDto dto)
        {
            try
            {
                var item = await _context.InventarioItems.FindAsync(id);
                if (item == null) return NotFound(new { message = "Producto no encontrado" });

                var nombreDuplicado = await _context.InventarioItems
                    .AnyAsync(p => p.Nombre.ToLower() == dto.Nombre.ToLower() && p.Id != id);

                if (nombreDuplicado)
                    return BadRequest(new { message = "Ya existe otro producto con ese nombre" });

                // Actualizar Categoría si cambió
                var categoria = await _context.Categorias
                    .FirstOrDefaultAsync(c => c.Nombre.ToLower() == dto.Categoria.ToLower());
                
                if (categoria == null)
                {
                    categoria = new Categoria { Nombre = dto.Categoria };
                    _context.Categorias.Add(categoria);
                    await _context.SaveChangesAsync();
                }

                item.Nombre = dto.Nombre;
                item.CategoriaId = categoria.Id;
                item.CostoUnitario = dto.PrecioUnitario;
                item.StockActual = dto.Stock;
                item.StockMinimo = dto.StockMinimo;
                item.UnidadMedida = dto.UnidadMedida ?? "Unidad";
                item.Activo = dto.Activo;
                // item.FechaActualizacion = DateTime.Now; // Si tienes este campo, descoméntalo

                await _context.SaveChangesAsync();

                return Ok(new { message = "Producto actualizado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar producto {Id}", id);
                return StatusCode(500, new { message = "Error al actualizar producto" });
            }
        }

        // DELETE: api/Inventario/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> EliminarProducto(int id)
        {
            try
            {
                var item = await _context.InventarioItems.FindAsync(id);
                if (item == null) return NotFound(new { message = "Producto no encontrado" });

                item.Activo = false;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Producto desactivado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar producto {Id}", id);
                return StatusCode(500, new { message = "Error al eliminar producto" });
            }
        }

        // GET: api/Inventario/movimientos-recientes
        [HttpGet("movimientos-recientes")]
        public async Task<ActionResult<IEnumerable<MovimientoDto>>> GetMovimientosRecientes([FromQuery] int cantidad = 20)
        {
            try
            {
                var movimientos = await _context.InventarioMovimientos
                    .Include(m => m.InventarioItem)
                    .OrderByDescending(m => m.Fecha)
                    .Take(cantidad)
                    .Select(m => new MovimientoDto
                    {
                        Id = m.Id,
                        Fecha = m.Fecha,
                        ProductoNombre = m.InventarioItem != null ? m.InventarioItem.Nombre : "Item Eliminado",
                        Tipo = m.Tipo,
                        Cantidad = m.Cantidad,
                        CostoUnitario = m.CostoUnitario,
                        Motivo = m.Motivo ?? "Sin motivo"
                    })
                    .ToListAsync();

                return Ok(movimientos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener movimientos recientes");
                return StatusCode(500, new { message = "Error al obtener movimientos" });
            }
        }

        // POST: api/Inventario/registrar-movimiento
        [HttpPost("registrar-movimiento")]
        public async Task<ActionResult> RegistrarMovimiento([FromBody] RegistrarMovimientoDto dto)
        {
            try
            {
                var item = await _context.InventarioItems.FindAsync(dto.ProductoId);
                if (item == null) return NotFound(new { message = "Producto no encontrado" });

                if (dto.Cantidad <= 0) return BadRequest(new { message = "La cantidad debe ser mayor a 0" });

                if (dto.Tipo.ToUpper() == "SALIDA" && item.StockActual < dto.Cantidad)
                    return BadRequest(new { message = $"Stock insuficiente. Disp: {item.StockActual}" });

                if (dto.Tipo.ToUpper() == "ENTRADA")
                {
                    item.StockActual += dto.Cantidad;
                    if (dto.CostoUnitario.HasValue && dto.CostoUnitario > 0)
                        item.CostoUnitario = dto.CostoUnitario.Value;
                }
                else // SALIDA
                {
                    item.StockActual -= dto.Cantidad;
                }

                var movimiento = new InventarioMovimiento
                {
                    ItemId = item.Id,
                    Tipo = dto.Tipo,
                    Cantidad = dto.Cantidad,
                    CostoUnitario = item.CostoUnitario,
                    Motivo = dto.Motivo ?? "Movimiento Manual",
                    Fecha = DateTime.Now
                };

                _context.InventarioMovimientos.Add(movimiento);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Movimiento registrado", nuevoStock = item.StockActual });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar movimiento");
                return StatusCode(500, new { message = "Error interno" });
            }
        }

        // GET: api/Inventario/categorias
        [HttpGet("categorias")]
        public async Task<ActionResult<IEnumerable<string>>> GetCategorias()
        {
            return Ok(await _context.Categorias.Select(c => c.Nombre).ToListAsync());
        }

        // --- DTOs ACTUALIZADOS (Inicializados para evitar CS8618) ---

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