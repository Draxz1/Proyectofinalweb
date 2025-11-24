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
        private readonly ApplicationDbContext _context;

        public InventarioController(ApplicationDbContext context)
        {
            _context = context;
        }

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
        }
    }
}
