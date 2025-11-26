using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using megadeliciasapi.Data;
using megadeliciasapi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using megadeliciasapi.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace megadeliciasapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventarioController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public InventarioController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // LISTADO PRINCIPAL (frontend: tabla de inventario)
        // Usa InventarioItemDto → incluye nombre de categoría como string
        // GET: api/inventario?busqueda=...
        // ==========================================
        [HttpGet]
        public async Task<ActionResult<List<InventarioItemDto>>> GetInventario([FromQuery] string busqueda = "")
        {
            var query = _context.InventarioItems
                .Include(i => i.Categoria)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                query = query.Where(i =>
                    i.Nombre.Contains(busqueda) ||
                    (i.Codigo != null && i.Codigo.Contains(busqueda)));
            }

            var items = await query.Select(i => new InventarioItemDto
            {
                Id = i.Id,
                Codigo = i.Codigo ?? "---",
                Nombre = i.Nombre,
                Categoria = i.Categoria != null ? i.Categoria.Nombre : "---",
                StockActual = i.StockActual,
                CostoUnitario = i.CostoUnitario,
                UnidadMedida = i.UnidadMedida,
                StockMinimo = i.StockMinimo,
                Activo = i.Activo,
                CreadoEn = i.CreadoEn,
                CategoriaId = i.CategoriaId
            }).ToListAsync();

            return Ok(items);
        }

        // ==========================================
        // DETALLE COMPLETO (admin)
        // GET: api/inventario/{id}
        // ==========================================
        [HttpGet("{id:int}")]
        public async Task<ActionResult<InventarioItemDto>> GetInventarioItem(int id)
        {
            var item = await _context.InventarioItems
                .Include(i => i.Categoria)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (item == null) return NotFound();

            var responseDto = new InventarioItemDto
            {
                Id = item.Id,
                Codigo = item.Codigo,
                Nombre = item.Nombre,
                StockActual = item.StockActual,
                StockMinimo = item.StockMinimo,
                CostoUnitario = item.CostoUnitario,
                UnidadMedida = item.UnidadMedida,
                Activo = item.Activo,
                CreadoEn = item.CreadoEn,
                CategoriaId = item.CategoriaId,
                Categoria = item.Categoria?.Nombre
            };

            return Ok(responseDto);
        }

        // ==========================================
        // MOVIMIENTO (entrada/salida)
        // POST: api/inventario/movimiento
        // ==========================================
        [HttpPost("movimiento")]
        public async Task<IActionResult> RegistrarMovimiento([FromBody] RegistrarMovimientoDto dto)
        {
            if (dto == null) return BadRequest("Datos inválidos.");

            var item = await _context.InventarioItems.FindAsync(dto.ItemId);
            if (item == null) return NotFound("El producto no existe.");

            var tipo = (dto.Tipo ?? string.Empty).Trim().ToUpperInvariant();

            if (tipo == "ENTRADA")
            {
                item.StockActual += dto.Cantidad;
                if (dto.CostoUnitario.HasValue && dto.CostoUnitario.Value > 0)
                    item.CostoUnitario = dto.CostoUnitario.Value;
            }
            else if (tipo == "SALIDA")
            {
                if (item.StockActual < dto.Cantidad)
                    return BadRequest($"Stock insuficiente. Solo tienes {item.StockActual} {item.UnidadMedida}.");

                item.StockActual -= dto.Cantidad;
            }
            else
            {
                return BadRequest("Tipo de movimiento inválido. Use 'ENTRADA' o 'SALIDA'.");
            }

            var movimiento = new InventarioMovimiento
            {
                ItemId = item.Id,
                Tipo = dto.Tipo,
                Cantidad = dto.Cantidad,
                CostoUnitario = item.CostoUnitario,
                Motivo = dto.Motivo,
                Fecha = DateTime.Now
            };

            _context.InventarioMovimientos.Add(movimiento);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Movimiento registrado", nuevoStock = item.StockActual });
        }

        // ==========================================
        // VERIFICAR PLATO
        // GET: api/inventario/verificar-plato/{platoId}
        // ==========================================
        [HttpGet("verificar-plato/{platoId:int}")]
        public async Task<ActionResult<DisponibilidadPlatoDto>> VerificarPlato(int platoId)
        {
            var receta = await _context.PlatoIngredientes
                .Where(pi => pi.PlatoId == platoId)
                .Include(pi => pi.InventarioItem)
                .ToListAsync();

            var respuesta = new DisponibilidadPlatoDto
            {
                PlatoId = platoId,
                EstaDisponible = true,
                IngredientesFaltantes = new List<string>()
            };

            foreach (var ingrediente in receta)
            {
                int necesario = (int)Math.Ceiling(ingrediente.CantidadUsada);
                if (ingrediente.InventarioItem == null || ingrediente.InventarioItem.StockActual < necesario)
                {
                    respuesta.EstaDisponible = false;
                    respuesta.IngredientesFaltantes.Add(ingrediente.InventarioItem?.Nombre ?? "Ingrediente desconocido");
                }
            }

            return Ok(respuesta);
        }

        // ==========================================
        // PROCESAR ORDEN
        // POST: api/inventario/procesar-orden/{ordenId}
        // ==========================================
        [HttpPost("procesar-orden/{ordenId:int}")]
        public async Task<IActionResult> ProcesarOrden(int ordenId)
        {
            await using var transaccion = await _context.Database.BeginTransactionAsync();
            try
            {
                var detalles = await _context.DetalleOrdenes
                    .Where(d => d.OrdenId == ordenId)
                    .ToListAsync();

                foreach (var detalle in detalles)
                {
                    var ingredientes = await _context.PlatoIngredientes
                        .Where(pi => pi.PlatoId == detalle.PlatoId)
                        .Include(pi => pi.InventarioItem)
                        .ToListAsync();

                    foreach (var ing in ingredientes)
                    {
                        if (ing.InventarioItem == null)
                            throw new Exception($"Ingrediente no encontrado para PlatoId {ing.PlatoId}");

                        int totalADescontar = (int)Math.Ceiling(ing.CantidadUsada * detalle.Cantidad);
                        if (ing.InventarioItem.StockActual < totalADescontar)
                            throw new Exception($"Sin stock de {ing.InventarioItem.Nombre} para completar la orden.");

                        ing.InventarioItem.StockActual -= totalADescontar;

                        _context.InventarioMovimientos.Add(new InventarioMovimiento
                        {
                            ItemId = ing.ItemId,
                            Tipo = "SALIDA",
                            Cantidad = totalADescontar,
                            CostoUnitario = ing.InventarioItem.CostoUnitario,
                            Motivo = $"Venta Orden #{ordenId}",
                            Fecha = DateTime.Now
                        });
                    }
                }

                await _context.SaveChangesAsync();
                await transaccion.CommitAsync();
                return Ok(new { mensaje = "Inventario actualizado correctamente." });
            }
            catch (Exception ex)
            {
                await transaccion.RollbackAsync();
                return BadRequest(ex.Message);
            }
        }

        // ==========================================
        // MOVIMIENTOS RECIENTES
        // GET: api/inventario/movimientos
        // ==========================================
        [HttpGet("movimientos")]
public async Task<ActionResult<List<MovimientoDto>>> GetMovimientosRecientes()
{
    var movimientos = await _context.InventarioMovimientos
        .Include(m => m.InventarioItem)
        .OrderByDescending(m => m.Fecha)
        .Take(20)
        .Select(m => new MovimientoDto
        {
            Id = m.Id,
            Fecha = m.Fecha,
            ItemNombre = m.InventarioItem != null ? m.InventarioItem.Nombre : "---",
            Tipo = m.Tipo,
            Cantidad = m.Cantidad,
            CostoUnitario = m.CostoUnitario,
            Motivo = m.Motivo != null ? m.Motivo : "---"
        })
        .ToListAsync();

    return Ok(movimientos);
}


        // ==========================================
        // CATEGORÍAS
        // GET: api/inventario/categorias
        // ==========================================
        [HttpGet("categorias")]
        public async Task<ActionResult<List<Categoria>>> GetCategorias()
        {
            return await _context.Categorias.ToListAsync();
        }

        // ==========================================
        // CREAR ÍTEM (admin)
        // POST: api/inventario/item
        // ==========================================
        [HttpPost("item")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> CrearItem([FromBody] CrearItemDto dto)
        {
            if (dto == null) return BadRequest("Datos inválidos.");

            if (await _context.InventarioItems.AnyAsync(i => i.Nombre == dto.Nombre))
                return BadRequest("Ya existe un producto con ese nombre.");

            var nuevoItem = new InventarioItem
            {
                Codigo = dto.Codigo,
                Nombre = dto.Nombre,
                CategoriaId = dto.CategoriaId,
                UnidadMedida = dto.UnidadMedida,
                StockMinimo = dto.StockMinimo,
                StockActual = dto.StockActual ?? 0,
                CostoUnitario = dto.CostoUnitario ?? 0m,
                Activo = dto.Activo ?? true,
                CreadoEn = DateTime.Now
            };

            _context.InventarioItems.Add(nuevoItem);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Producto creado exitosamente", id = nuevoItem.Id });
        }

        // ==========================================
        // ACTUALIZAR ÍTEM (admin)
        // PUT: api/inventario/{id}
        // ==========================================
        [HttpPut("{id:int}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> ActualizarItem(int id, [FromBody] CrearItemDto dto)
        {
            if (dto == null) return BadRequest("Datos inválidos.");

            var item = await _context.InventarioItems.FindAsync(id);
            if (item == null) return NotFound();

            item.Codigo = dto.Codigo;
            item.Nombre = dto.Nombre;
            item.CategoriaId = dto.CategoriaId;
            item.UnidadMedida = dto.UnidadMedida;
            item.StockMinimo = dto.StockMinimo;
            item.StockActual = dto.StockActual ?? item.StockActual;
            item.CostoUnitario = dto.CostoUnitario ?? item.CostoUnitario;
            item.Activo = dto.Activo ?? item.Activo;

            _context.Entry(item).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ==========================================
        // ELIMINAR ÍTEM (admin)
        // DELETE: api/inventario/{id}
        // ==========================================
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> EliminarItem(int id)
        {
            var item = await _context.InventarioItems.FindAsync(id);
            if (item == null) return NotFound();

            _context.InventarioItems.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
