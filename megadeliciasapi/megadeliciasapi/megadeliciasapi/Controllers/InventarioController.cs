using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using megadeliciasapi.Data;
using megadeliciasapi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using megadeliciasapi.DTOs;


namespace megadeliciasapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventarioController : ControllerBase
    {
 feature/inventario-completo
        // ⚠️ NOTA: Usamos 'ApplicationDbContext' (en inglés) tal como está en tu carpeta Data
=======
 main
        private readonly ApplicationDbContext _context;

        public InventarioController(ApplicationDbContext context)
        {
            _context = context;
        }

 feature/inventario-completo
        // ==========================================
        // 1. GET: Obtener lista de items (Tabla)
        // ==========================================
        [HttpGet]
        public async Task<ActionResult<List<InventarioItemDto>>> GetInventario([FromQuery] string busqueda = "")
        {
            var query = _context.InventarioItems
                .Include(i => i.Categoria) // Traemos la categoría para mostrar el nombre
                .AsQueryable();

            if (!string.IsNullOrEmpty(busqueda))
            {
                query = query.Where(i => i.Nombre.Contains(busqueda) || (i.Codigo != null && i.Codigo.Contains(busqueda)));
            }

            var items = await query.Select(i => new InventarioItemDto
            {
                Id = i.Id,
                Codigo = i.Codigo ?? "---",
                Nombre = i.Nombre,
                Categoria = i.Categoria.Nombre,
                StockActual = i.StockActual,
                CostoUnitario = i.CostoUnitario,
                UnidadMedida = i.UnidadMedida
            }).ToListAsync();

            return Ok(items);
        }

        // ==========================================
        // 2. POST: Registrar Entrada/Salida
        // ==========================================
        [HttpPost("movimiento")]
        public async Task<IActionResult> RegistrarMovimiento([FromBody] RegistrarMovimientoDto dto)
        {
            var item = await _context.InventarioItems.FindAsync(dto.ItemId);
            if (item == null) return NotFound("El producto no existe.");

            string tipo = dto.Tipo.ToUpper();

            // Actualizar Stock en la tabla principal
            if (tipo == "ENTRADA")
            {
                item.StockActual += dto.Cantidad;
                // Si ingresan un costo nuevo, actualizamos el costo unitario del producto
                if (dto.CostoUnitario > 0) item.CostoUnitario = dto.CostoUnitario;
            }
            else if (tipo == "SALIDA")
            {
                if (item.StockActual < dto.Cantidad)
                    return BadRequest($"Stock insuficiente. Solo tienes {item.StockActual} {item.UnidadMedida}.");

                item.StockActual -= dto.Cantidad;
            }

            // Guardar historial del movimiento
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
        // 3. LOGICA MESERO: ¿Hay ingredientes?
        // ==========================================
        [HttpGet("verificar-plato/{platoId}")]
        public async Task<ActionResult<DisponibilidadPlatoDto>> VerificarPlato(int platoId)
        {
            // Buscamos la receta del plato
            var receta = await _context.PlatoIngredientes
                .Where(pi => pi.PlatoId == platoId)
                .Include(pi => pi.InventarioItem)
                .ToListAsync();

            var respuesta = new DisponibilidadPlatoDto { PlatoId = platoId, EstaDisponible = true };

            foreach (var ingrediente in receta)
            {
                // Convertimos decimal a entero redondeando hacia arriba para asegurar stock
                int necesario = (int)Math.Ceiling(ingrediente.CantidadUsada);

                if (ingrediente.InventarioItem.StockActual < necesario)
                {
                    respuesta.EstaDisponible = false;
                    respuesta.IngredientesFaltantes.Add(ingrediente.InventarioItem.Nombre);
                }
            }

            return Ok(respuesta);
        }

        // ==========================================
        // 4. LOGICA COCINA: Descontar por Orden
        // ==========================================
        [HttpPost("procesar-orden/{ordenId}")]
        public async Task<IActionResult> ProcesarOrden(int ordenId)
        {
            using var transaccion = _context.Database.BeginTransaction();
            try
            {
                // Obtenemos los platos de la orden
                var detalles = await _context.DetalleOrdenes
                    .Where(d => d.OrdenId == ordenId)
                    .ToListAsync();

                foreach (var detalle in detalles)
                {
                    // Buscamos ingredientes de cada plato
                    var ingredientes = await _context.PlatoIngredientes
                        .Where(pi => pi.PlatoId == detalle.PlatoId)
                        .Include(pi => pi.InventarioItem)
                        .ToListAsync();

                    foreach (var ing in ingredientes)
                    {
                        // Total a descontar = (Receta * Cantidad de Platos Pedidos)
                        int totalADescontar = (int)Math.Ceiling(ing.CantidadUsada * detalle.Cantidad);

                        // Verificamos stock otra vez por seguridad
                        if (ing.InventarioItem.StockActual < totalADescontar)
                            throw new Exception($"Sin stock de {ing.InventarioItem.Nombre} para completar la orden.");

                        // Descontamos
                        ing.InventarioItem.StockActual -= totalADescontar;

                        // Registramos movimiento automático
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

        [HttpGet("movimientos")]
        public async Task<ActionResult<List<MovimientoDto>>> GetMovimientosRecientes()
        {
            var movimientos = await _context.InventarioMovimientos
                .Include(m => m.InventarioItem) // Relacionamos para sacar el nombre
                .OrderByDescending(m => m.Fecha) // Los más recientes primero
                .Take(20) // Solo los últimos 20
                .Select(m => new MovimientoDto
                {
                    Id = m.Id,
                    Fecha = m.Fecha,
                    ItemNombre = m.InventarioItem.Nombre,
                    Tipo = m.Tipo,
                    Cantidad = m.Cantidad,
                    CostoUnitario = m.CostoUnitario,
                    Motivo = m.Motivo ?? "---"
                })
                .ToListAsync();

            return Ok(movimientos);
        }

        [HttpGet("categorias")]
        public async Task<ActionResult<List<Categoria>>> GetCategorias()
        {
            return await _context.Categorias.ToListAsync();
        }

        // 7. CREAR NUEVO ITEM
        [HttpPost("item")]
        public async Task<IActionResult> CrearItem([FromBody] CrearItemDto dto)
        {
            if (await _context.InventarioItems.AnyAsync(i => i.Nombre == dto.Nombre))
                return BadRequest("Ya existe un producto con ese nombre.");

            var nuevoItem = new InventarioItem
            {
                Codigo = dto.Codigo,
                Nombre = dto.Nombre,
                CategoriaId = dto.CategoriaId,
                UnidadMedida = dto.UnidadMedida,
                StockMinimo = dto.StockMinimo,
                StockActual = 0,      // Empieza en 0
                CostoUnitario = 0,    // Empieza en 0
                Activo = true,
                CreadoEn = DateTime.Now
            };

            _context.InventarioItems.Add(nuevoItem);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Producto creado exitosamente", id = nuevoItem.Id });
=======
        // --- 1. READ ALL (Obtener todo el inventario) ---
        // GET: api/Inventario
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InventarioDTos>>> GetInventarioItems()
        {
            var inventarioItems = await _context.InventarioItems
                                                // Opcional: Incluir el nombre de la categoría si se necesita en el DTO
                                                .Include(i => i.Categoria)
                                                .OrderBy(i => i.Nombre)
                                                .ToListAsync();

            // Mapeo de la Entidad al DTO de Respuesta
            var responseDTOs = inventarioItems.Select(item => new InventarioDTos
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
                // Si agregaste NombreCategoria al DTO: NombreCategoria = item.Categoria.Nombre
            }).ToList();

            return responseDTOs;
        }

        // --- 2. READ BY ID (Obtener un ítem específico) ---
        // GET: api/Inventario/5
        [HttpGet("{id}")]
        public async Task<ActionResult<InventarioDTos>> GetInventarioItem(int id)
        {
            var item = await _context.InventarioItems
                                     // Opcional: Incluir la Categoría
                                     .Include(i => i.Categoria)
                                     .FirstOrDefaultAsync(i => i.Id == id);

            if (item == null)
            {
                return NotFound();
            }

            // Mapeo de la Entidad al DTO de Respuesta
            var responseDTO = new InventarioDTos
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
                CategoriaId = item.CategoriaId
            };

            return responseDTO;
        }

        // --- 3. CREATE (Ingresar ítem - Solo Admin) ---
        // POST: api/Inventario
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<InventarioDTos>> PostInventarioItem(CrearInventarioDTOs requestDto)
        {
            if (requestDto == null) return BadRequest();

            // Mapeo del DTO de Request a la Entidad
            var item = new InventarioItem
            {
                Codigo = requestDto.Codigo,
                Nombre = requestDto.Nombre,
                StockActual = requestDto.StockActual,
                StockMinimo = requestDto.StockMinimo,
                CostoUnitario = requestDto.CostoUnitario,
                UnidadMedida = requestDto.UnidadMedida,
                Activo = requestDto.Activo,
                CategoriaId = requestDto.CategoriaId,
                CreadoEn = DateTime.Now // El servidor establece la fecha de creación
            };
            
            _context.InventarioItems.Add(item);
            await _context.SaveChangesAsync();

            // Mapeamos la Entidad creada de vuelta a un Response DTO para el resultado
            var responseDto = new InventarioDTos
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
                CategoriaId = item.CategoriaId
            };

            return CreatedAtAction(nameof(GetInventarioItem), new { id = item.Id }, responseDto);
        }

        // --- 4. UPDATE (Editar ítem - Solo Admin) ---
        // PUT: api/Inventario/5
        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> PutInventarioItem(int id, CrearInventarioDTOs requestDto)
        {
            // 1. Encontrar el ítem existente
            var itemToUpdate = await _context.InventarioItems.FindAsync(id);
            if (itemToUpdate == null)
            {
                return NotFound();
            }

            // 2. Aplicar los cambios del DTO al objeto de la Entidad
            itemToUpdate.Codigo = requestDto.Codigo;
            itemToUpdate.Nombre = requestDto.Nombre;
            // Ojo: Si el stock se actualiza con movimientos, este campo podría ser sensible
            itemToUpdate.StockActual = requestDto.StockActual; 
            itemToUpdate.StockMinimo = requestDto.StockMinimo;
            itemToUpdate.CostoUnitario = requestDto.CostoUnitario;
            itemToUpdate.UnidadMedida = requestDto.UnidadMedida;
            itemToUpdate.Activo = requestDto.Activo;
            itemToUpdate.CategoriaId = requestDto.CategoriaId;

            _context.Entry(itemToUpdate).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Manejo de concurrencia: si no existe (raro aquí), lanza excepción
                if (!_context.InventarioItems.Any(e => e.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // --- 5. DELETE (Borrar ítem - Solo Admin) ---
        // DELETE: api/Inventario/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteInventarioItem(int id)
        {
            var item = await _context.InventarioItems.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            _context.InventarioItems.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
 main
        }
    }
}