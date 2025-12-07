using megadeliciasapi.Data;
using megadeliciasapi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace megadeliciasapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SeedController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SeedController> _logger;

        public SeedController(ApplicationDbContext context, ILogger<SeedController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ========================================
        // 1. SEMBRAR ESTADOS
        // ========================================
        [HttpPost("estados")]
        public async Task<IActionResult> SeedEstados()
        {
            if (await _context.EstadosOrden.AnyAsync())
            {
                return Ok(new { message = "Los estados ya existen en la base de datos." });
            }

            var estados = new List<EstadoOrden>
            {
                new EstadoOrden { Nombre = "PENDIENTE", Descripcion = "Orden creada, esperando cocina" },
                new EstadoOrden { Nombre = "EN_PROCESO", Descripcion = "Cocina está preparando los alimentos" },
                new EstadoOrden { Nombre = "LISTO", Descripcion = "Alimentos preparados, esperando mesero" },
                new EstadoOrden { Nombre = "ENTREGADO", Descripcion = "Orden entregada al cliente" },
                new EstadoOrden { Nombre = "CANCELADO", Descripcion = "Orden cancelada" }
            };

            await _context.EstadosOrden.AddRangeAsync(estados);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"✅ Se agregaron {estados.Count} estados exitosamente." });
        }

        // ========================================
        // 2. SEMBRAR MENÚ
        // ========================================
        [HttpPost("menu")]
        public async Task<IActionResult> SeedMenu()
        {
            if (await _context.Platos.AnyAsync())
            {
                return Ok(new { message = "⚠️ El menú ya tiene datos. No se hizo nada." });
            }

            var menu = new List<Plato>
            {
                // Delibaleadas
                new Plato { Nombre = "Mega Delis", Precio = 90, Categoria = "Delibaleadas", Estado = "Activo", Disponible = true },
                new Plato { Nombre = "Delis", Precio = 60, Categoria = "Delibaleadas", Estado = "Activo", Disponible = true },

                // Desayunos
                new Plato { Nombre = "Catracho", Precio = 150, Categoria = "Desayunos", Estado = "Activo", Disponible = true },
                new Plato { Nombre = "Americano", Precio = 170, Categoria = "Desayunos", Estado = "Activo", Disponible = true },
                new Plato { Nombre = "Panqueque", Precio = 70, Categoria = "Desayunos", Estado = "Activo", Disponible = true },

                // Tortas
                new Plato { Nombre = "Torta Deli de camarones", Precio = 195, Categoria = "Tortas Deli", Estado = "Activo", Disponible = true },
                new Plato { Nombre = "Torta Deli de pollo", Precio = 150, Categoria = "Tortas Deli", Estado = "Activo", Disponible = true },
                new Plato { Nombre = "Torta Deli de jamón y queso", Precio = 140, Categoria = "Tortas Deli", Estado = "Activo", Disponible = true },

                // Almuerzos/Cenas
                new Plato { Nombre = "Almuerzo del día", Precio = 210, Categoria = "Almuerzo", Estado = "Activo", Disponible = true },
                new Plato { Nombre = "Cena típica", Precio = 150, Categoria = "Cena", Estado = "Activo", Disponible = true },

                // Compartir
                new Plato { Nombre = "Nachos (chili o pollo)", Precio = 180, Categoria = "Para compartir", Estado = "Activo", Disponible = true },
                new Plato { Nombre = "Badass Fries", Precio = 180, Categoria = "Para compartir", Estado = "Activo", Disponible = true },
                new Plato { Nombre = "Hamburguesa de la casa", Precio = 180, Categoria = "Hamburguesa", Estado = "Activo", Disponible = true },
                new Plato { Nombre = "Megatostones de la casa", Precio = 150, Categoria = "Megatostones", Estado = "Activo", Disponible = true },
                new Plato { Nombre = "Alitas de la casa (6)", Precio = 190, Categoria = "Alitas", Estado = "Activo", Disponible = true },
                new Plato { Nombre = "Alitas de la casa (12)", Precio = 360, Categoria = "Alitas", Estado = "Activo", Disponible = true },

                // Sándwich
                new Plato { Nombre = "Sándwich Cubano", Precio = 190, Categoria = "Sandwich", Estado = "Activo", Disponible = true },

                // Fetuccini
                new Plato { Nombre = "Fetuccini Alfredo", Precio = 210, Categoria = "Fetuccini", Estado = "Activo", Disponible = true },
                new Plato { Nombre = "Fetuccini con camarones", Precio = 280, Categoria = "Fetuccini", Estado = "Activo", Disponible = true },
                new Plato { Nombre = "Fetuccini con pollo", Precio = 250, Categoria = "Fetuccini", Estado = "Activo", Disponible = true },

                // Platos fuertes
                new Plato { Nombre = "Chuleta con tajadas (estilo costeño)", Precio = 210, Categoria = "Chuleta con tajadas", Estado = "Activo", Disponible = true },
                new Plato { Nombre = "Pollo chuco de la casa", Precio = 210, Categoria = "Pollo choco", Estado = "Activo", Disponible = true },
                new Plato { Nombre = "Chicken Fingers de la casa", Precio = 180, Categoria = "Chicken Finger", Estado = "Activo", Disponible = true },

                // Tostadas
                new Plato { Nombre = "Tostadas de camarón", Precio = 150, Categoria = "Tostadas", Estado = "Activo", Disponible = true },
                new Plato { Nombre = "Tostadas de pollo", Precio = 100, Categoria = "Tostadas", Estado = "Activo", Disponible = true },

                // Postres
                new Plato { Nombre = "Torta de arroz con leche", Precio = 95, Categoria = "Postres", Estado = "Activo", Disponible = true },
                new Plato { Nombre = "Cheesecake con salsa de vino tinto", Precio = 110, Categoria = "Postres", Estado = "Activo", Disponible = true },
                new Plato { Nombre = "Flan de coco", Precio = 55, Categoria = "Postres", Estado = "Activo", Disponible = true },
                new Plato { Nombre = "Pan de banano", Precio = 45, Categoria = "Postres", Estado = "Activo", Disponible = true },
                
                // Bebidas
                new Plato { Nombre = "Limonada", Precio = 45, Categoria = "Bebidas naturales", Estado = "Activo", Disponible = true },
                new Plato { Nombre = "Naranja", Precio = 45, Categoria = "Bebidas naturales", Estado = "Activo", Disponible = true },
                new Plato { Nombre = "Cerveza Corona", Precio = 70, Categoria = "Cervezas", Estado = "Activo", Disponible = true }
            };

            await _context.Platos.AddRangeAsync(menu);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"✅ Se agregaron {menu.Count} platos al menú exitosamente." });
        }

        // ========================================
        // 3. SEMBRAR MESAS
        // ========================================
        [HttpPost("mesas")]
        public async Task<IActionResult> SeedMesas()
        {
            if (await _context.Mesas.AnyAsync())
            {
                return Ok(new { message = "⚠️ Las mesas ya existen." });
            }

            var mesas = new List<Mesa>();
            for (int i = 1; i <= 10; i++)
            {
                mesas.Add(new Mesa 
                { 
                    Codigo = $"M-{i}", 
                    Capacidad = 4, 
                    Activa = true 
                });
            }

            await _context.Mesas.AddRangeAsync(mesas);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"✅ Se agregaron {mesas.Count} mesas." });
        }

        // ========================================
        // 4. SEMBRAR INVENTARIO (PRODUCTOS) ⭐ Panel General
        // ========================================
        [HttpPost("inventario")]
        public async Task<IActionResult> SeedInventario()
        {
            try
            {
                if (await _context.Productos.AnyAsync())
                {
                    return Ok(new { 
                        message = "⚠️ Ya existen productos en el inventario.",
                        totalProductos = await _context.Productos.CountAsync()
                    });
                }

                var productos = new List<Producto>
                {
                    // CARNES
                    new Producto { Nombre = "Carne Molida Especial", Descripcion = "Carne molida de res premium", Categoria = "Carnes", PrecioUnitario = 85.00m, Stock = 5, StockMinimo = 2, UnidadMedida = "Libra", Activo = true },
                    new Producto { Nombre = "Pechuga de Pollo", Descripcion = "Pechuga de pollo fresca sin hueso", Categoria = "Carnes", PrecioUnitario = 60.00m, Stock = 8, StockMinimo = 3, UnidadMedida = "Libra", Activo = true },
                    new Producto { Nombre = "Costilla de Cerdo", Descripcion = "Costilla de cerdo con hueso", Categoria = "Carnes", PrecioUnitario = 95.00m, Stock = 2, StockMinimo = 1, UnidadMedida = "Libra", Activo = true },
                    new Producto { Nombre = "Carne de Res en Bistec", Descripcion = "Bistec de res corte fino", Categoria = "Carnes", PrecioUnitario = 120.00m, Stock = 6, StockMinimo = 2, UnidadMedida = "Libra", Activo = true },
                    new Producto { Nombre = "Muslo de Pollo", Descripcion = "Muslo de pollo con piel", Categoria = "Carnes", PrecioUnitario = 45.00m, Stock = 10, StockMinimo = 4, UnidadMedida = "Libra", Activo = true },

                    // VERDURAS
                    new Producto { Nombre = "Tomate Manzano", Descripcion = "Tomate rojo fresco", Categoria = "Verduras", PrecioUnitario = 5.00m, Stock = 25, StockMinimo = 10, UnidadMedida = "Unidad", Activo = true },
                    new Producto { Nombre = "Cebolla Amarilla", Descripcion = "Cebolla amarilla mediana", Categoria = "Verduras", PrecioUnitario = 4.00m, Stock = 15, StockMinimo = 5, UnidadMedida = "Unidad", Activo = true },
                    new Producto { Nombre = "Chile Dulce", Descripcion = "Chile dulce verde o rojo", Categoria = "Verduras", PrecioUnitario = 6.00m, Stock = 11, StockMinimo = 5, UnidadMedida = "Unidad", Activo = true },
                    new Producto { Nombre = "Papa", Descripcion = "Papa blanca grande", Categoria = "Verduras", PrecioUnitario = 12.00m, Stock = 50, StockMinimo = 20, UnidadMedida = "Libra", Activo = true },
                    new Producto { Nombre = "Culantro", Descripcion = "Culantro fresco", Categoria = "Verduras", PrecioUnitario = 3.00m, Stock = 5, StockMinimo = 2, UnidadMedida = "Manojo", Activo = true },
                    new Producto { Nombre = "Lechuga Americana", Descripcion = "Lechuga fresca", Categoria = "Verduras", PrecioUnitario = 15.00m, Stock = 8, StockMinimo = 3, UnidadMedida = "Unidad", Activo = true },
                    new Producto { Nombre = "Zanahoria", Descripcion = "Zanahoria fresca", Categoria = "Verduras", PrecioUnitario = 8.00m, Stock = 20, StockMinimo = 8, UnidadMedida = "Libra", Activo = true },

                    // FRUTAS
                    new Producto { Nombre = "Banano Maduro", Descripcion = "Banano maduro para freír", Categoria = "Frutas", PrecioUnitario = 2.00m, Stock = 30, StockMinimo = 10, UnidadMedida = "Unidad", Activo = true },
                    new Producto { Nombre = "Limón", Descripcion = "Limón persa fresco", Categoria = "Frutas", PrecioUnitario = 3.00m, Stock = 40, StockMinimo = 15, UnidadMedida = "Unidad", Activo = true },
                    new Producto { Nombre = "Aguacate", Descripcion = "Aguacate hass maduro", Categoria = "Frutas", PrecioUnitario = 18.00m, Stock = 12, StockMinimo = 5, UnidadMedida = "Unidad", Activo = true },
                    new Producto { Nombre = "Piña", Descripcion = "Piña golden madura", Categoria = "Frutas", PrecioUnitario = 35.00m, Stock = 5, StockMinimo = 2, UnidadMedida = "Unidad", Activo = true },

                    // LÁCTEOS
                    new Producto { Nombre = "Queso Crema", Descripcion = "Queso crema para untar", Categoria = "Lácteos", PrecioUnitario = 85.00m, Stock = 6, StockMinimo = 2, UnidadMedida = "Libra", Activo = true },
                    new Producto { Nombre = "Mantequilla", Descripcion = "Mantequilla con sal", Categoria = "Lácteos", PrecioUnitario = 45.00m, Stock = 4, StockMinimo = 2, UnidadMedida = "Libra", Activo = true },
                    new Producto { Nombre = "Leche Entera", Descripcion = "Leche entera pasteurizada", Categoria = "Lácteos", PrecioUnitario = 25.00m, Stock = 10, StockMinimo = 4, UnidadMedida = "Litro", Activo = true },
                    new Producto { Nombre = "Crema Ácida", Descripcion = "Crema ácida natural", Categoria = "Lácteos", PrecioUnitario = 35.00m, Stock = 8, StockMinimo = 3, UnidadMedida = "Litro", Activo = true },

                    // GRANOS
                    new Producto { Nombre = "Arroz Blanco", Descripcion = "Arroz blanco de primera", Categoria = "Granos", PrecioUnitario = 18.00m, Stock = 20, StockMinimo = 10, UnidadMedida = "Libra", Activo = true },
                    new Producto { Nombre = "Frijoles Rojos", Descripcion = "Frijoles rojos secos", Categoria = "Granos", PrecioUnitario = 22.00m, Stock = 15, StockMinimo = 8, UnidadMedida = "Libra", Activo = true },
                    new Producto { Nombre = "Harina de Trigo", Descripcion = "Harina de trigo todo uso", Categoria = "Granos", PrecioUnitario = 15.00m, Stock = 12, StockMinimo = 5, UnidadMedida = "Libra", Activo = true },
                    new Producto { Nombre = "Maíz en Grano", Descripcion = "Maíz amarillo seco", Categoria = "Granos", PrecioUnitario = 12.00m, Stock = 10, StockMinimo = 5, UnidadMedida = "Libra", Activo = true },

                    // ACEITES Y CONDIMENTOS
                    new Producto { Nombre = "Aceite Vegetal", Descripcion = "Aceite vegetal para cocinar", Categoria = "Aceites", PrecioUnitario = 75.00m, Stock = 6, StockMinimo = 2, UnidadMedida = "Litro", Activo = true },
                    new Producto { Nombre = "Sal", Descripcion = "Sal de cocina refinada", Categoria = "Condimentos", PrecioUnitario = 8.00m, Stock = 15, StockMinimo = 5, UnidadMedida = "Libra", Activo = true },
                    new Producto { Nombre = "Pimienta Negra", Descripcion = "Pimienta negra molida", Categoria = "Condimentos", PrecioUnitario = 45.00m, Stock = 3, StockMinimo = 1, UnidadMedida = "Libra", Activo = true },
                    new Producto { Nombre = "Ajo en Polvo", Descripcion = "Ajo deshidratado en polvo", Categoria = "Condimentos", PrecioUnitario = 35.00m, Stock = 4, StockMinimo = 2, UnidadMedida = "Libra", Activo = true },
                    new Producto { Nombre = "Comino", Descripcion = "Comino molido", Categoria = "Condimentos", PrecioUnitario = 40.00m, Stock = 2, StockMinimo = 1, UnidadMedida = "Libra", Activo = true },

                    // BEBIDAS
                    new Producto { Nombre = "Coca Cola 2.5L", Descripcion = "Refresco Coca Cola 2.5 litros", Categoria = "Bebidas", PrecioUnitario = 35.00m, Stock = 20, StockMinimo = 10, UnidadMedida = "Unidad", Activo = true },
                    new Producto { Nombre = "Sprite 2.5L", Descripcion = "Refresco Sprite 2.5 litros", Categoria = "Bebidas", PrecioUnitario = 35.00m, Stock = 15, StockMinimo = 8, UnidadMedida = "Unidad", Activo = true },
                    new Producto { Nombre = "Agua Embotellada 500ml", Descripcion = "Agua purificada embotellada", Categoria = "Bebidas", PrecioUnitario = 8.00m, Stock = 50, StockMinimo = 20, UnidadMedida = "Unidad", Activo = true },
                    new Producto { Nombre = "Jugo de Naranja Natural", Descripcion = "Jugo de naranja fresco", Categoria = "Bebidas", PrecioUnitario = 25.00m, Stock = 10, StockMinimo = 5, UnidadMedida = "Litro", Activo = true },

                    // PANADERÍA
                    new Producto { Nombre = "Pan Francés", Descripcion = "Pan francés fresco", Categoria = "Panadería", PrecioUnitario = 1.50m, Stock = 50, StockMinimo = 20, UnidadMedida = "Unidad", Activo = true },
                    new Producto { Nombre = "Pan de Hamburguesa", Descripcion = "Pan para hamburguesa con ajonjolí", Categoria = "Panadería", PrecioUnitario = 8.00m, Stock = 24, StockMinimo = 12, UnidadMedida = "Unidad", Activo = true },
                    new Producto { Nombre = "Tortilla de Harina", Descripcion = "Tortilla de harina grande", Categoria = "Panadería", PrecioUnitario = 2.00m, Stock = 40, StockMinimo = 20, UnidadMedida = "Unidad", Activo = true }
                };

                _context.Productos.AddRange(productos);
                await _context.SaveChangesAsync();

                var resumen = productos
                    .GroupBy(p => p.Categoria)
                    .Select(g => new { Categoria = g.Key, Cantidad = g.Count() })
                    .ToList();

                return Ok(new { 
                    message = "✅ Inventario inicial creado exitosamente",
                    totalProductos = productos.Count,
                    resumenPorCategoria = resumen
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear inventario inicial");
                return StatusCode(500, new { message = "❌ Error al crear inventario", error = ex.Message });
            }
        }

        // ========================================
        // 5. SEMBRAR INGREDIENTES (InventarioItems) ⭐ NUEVO - Para Recetas
        // ========================================
        [HttpPost("inventario-items")]
        public async Task<IActionResult> SeedInventarioItems()
        {
            try
            {
                if (await _context.InventarioItems.AnyAsync())
                {
                    return Ok(new { 
                        message = "⚠️ Los ingredientes ya existen. Usa DELETE /api/Seed/inventario-items si quieres recrearlos.",
                        totalIngredientes = await _context.InventarioItems.CountAsync()
                    });
                }

                // Obtener o crear categoría
                var categoriaIngredientes = await _context.Categorias
                    .FirstOrDefaultAsync(c => c.Nombre == "Ingredientes");
                
                if (categoriaIngredientes == null)
                {
                    categoriaIngredientes = new Categoria { Nombre = "Ingredientes" };
                    _context.Categorias.Add(categoriaIngredientes);
                    await _context.SaveChangesAsync();
                }

                var ingredientes = new List<InventarioItem>
                {
                    // Verduras
                    new InventarioItem { 
                        Nombre = "Lechuga Romana", 
                        StockActual = 50, 
                        StockMinimo = 10, 
                        CostoUnitario = 15m, 
                        UnidadMedida = "Unidad", 
                        CategoriaId = categoriaIngredientes.Id 
                    },
                    new InventarioItem { 
                        Nombre = "Tomate Fresco", 
                        StockActual = 100, 
                        StockMinimo = 20, 
                        CostoUnitario = 8m, 
                        UnidadMedida = "Unidad", 
                        CategoriaId = categoriaIngredientes.Id 
                    },
                    new InventarioItem { 
                        Nombre = "Cebolla Blanca", 
                        StockActual = 80, 
                        StockMinimo = 15, 
                        CostoUnitario = 5m, 
                        UnidadMedida = "Unidad", 
                        CategoriaId = categoriaIngredientes.Id 
                    },
                    
                    // Carnes
                    new InventarioItem { 
                        Nombre = "Carne Molida de Res", 
                        StockActual = 30, 
                        StockMinimo = 5, 
                        CostoUnitario = 120m, 
                        UnidadMedida = "Libra", 
                        CategoriaId = categoriaIngredientes.Id 
                    },
                    new InventarioItem { 
                        Nombre = "Pechuga de Pollo", 
                        StockActual = 25, 
                        StockMinimo = 5, 
                        CostoUnitario = 85m, 
                        UnidadMedida = "Libra", 
                        CategoriaId = categoriaIngredientes.Id 
                    },
                    
                    // Panadería
                    new InventarioItem { 
                        Nombre = "Pan de Hamburguesa", 
                        StockActual = 100, 
                        StockMinimo = 20, 
                        CostoUnitario = 4m, 
                        UnidadMedida = "Unidad", 
                        CategoriaId = categoriaIngredientes.Id 
                    },
                    new InventarioItem { 
                        Nombre = "Pan Francés", 
                        StockActual = 80, 
                        StockMinimo = 15, 
                        CostoUnitario = 2m, 
                        UnidadMedida = "Unidad", 
                        CategoriaId = categoriaIngredientes.Id 
                    },
                    
                    // Lácteos
                    new InventarioItem { 
                        Nombre = "Queso Amarillo", 
                        StockActual = 20, 
                        StockMinimo = 5, 
                        CostoUnitario = 35m, 
                        UnidadMedida = "Libra", 
                        CategoriaId = categoriaIngredientes.Id 
                    },
                    new InventarioItem { 
                        Nombre = "Mantequilla", 
                        StockActual = 15, 
                        StockMinimo = 3, 
                        CostoUnitario = 45m, 
                        UnidadMedida = "Libra", 
                        CategoriaId = categoriaIngredientes.Id 
                    }
                };

                _context.InventarioItems.AddRange(ingredientes);
                await _context.SaveChangesAsync();

                return Ok(new { 
                    message = "✅ Ingredientes creados exitosamente",
                    totalIngredientes = ingredientes.Count,
                    ingredientes = ingredientes.Select(i => new { 
                        i.Id,
                        i.Nombre, 
                        i.StockActual, 
                        i.UnidadMedida 
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear ingredientes");
                return StatusCode(500, new { message = "❌ Error al crear ingredientes", error = ex.Message });
            }
        }

        // ========================================
        // 6. SEMBRAR RECETAS (PlatoIngrediente) ⭐ NUEVO
        // ========================================
        [HttpPost("recetas-ingredientes")]
        public async Task<IActionResult> SeedRecetasIngredientes()
        {
            try
            {
                if (await _context.PlatoIngredientes.AnyAsync())
                {
                    return Ok(new { 
                        message = "⚠️ Las recetas ya existen. Usa DELETE /api/Seed/recetas-ingredientes si quieres recrearlas.",
                        totalRecetas = await _context.PlatoIngredientes.CountAsync()
                    });
                }

                // Buscar plato Hamburguesa
                var hamburguesa = await _context.Platos
                    .FirstOrDefaultAsync(p => p.Nombre.Contains("Hamburguesa"));

                if (hamburguesa == null)
                {
                    return BadRequest(new { 
                        message = "❌ No se encontró el plato 'Hamburguesa' en el menú. Ejecuta primero POST /api/Seed/menu" 
                    });
                }

                // Buscar ingredientes
                var lechuga = await _context.InventarioItems.FirstOrDefaultAsync(i => i.Nombre.Contains("Lechuga"));
                var tomate = await _context.InventarioItems.FirstOrDefaultAsync(i => i.Nombre.Contains("Tomate"));
                var cebolla = await _context.InventarioItems.FirstOrDefaultAsync(i => i.Nombre.Contains("Cebolla"));
                var carne = await _context.InventarioItems.FirstOrDefaultAsync(i => i.Nombre.Contains("Carne"));
                var pan = await _context.InventarioItems.FirstOrDefaultAsync(i => i.Nombre.Contains("Pan de Hamburguesa"));

                if (lechuga == null || tomate == null || cebolla == null || carne == null || pan == null)
                {
                    return BadRequest(new { 
                        message = "❌ Faltan ingredientes. Ejecuta primero POST /api/Seed/inventario-items",
                        ingredientesEncontrados = new {
                            lechuga = lechuga?.Nombre ?? "NO ENCONTRADO",
                            tomate = tomate?.Nombre ?? "NO ENCONTRADO",
                            cebolla = cebolla?.Nombre ?? "NO ENCONTRADO",
                            carne = carne?.Nombre ?? "NO ENCONTRADO",
                            pan = pan?.Nombre ?? "NO ENCONTRADO"
                        }
                    });
                }

                // Crear recetas para Hamburguesa
                var recetas = new List<PlatoIngrediente>
                {
                    new PlatoIngrediente { 
                        PlatoId = hamburguesa.Id, 
                        ItemId = lechuga.Id, 
                        CantidadUsada = 1, 
                        UnidadMedida = "Unidad",
                        CreadoEn = DateTime.Now
                    },
                    new PlatoIngrediente { 
                        PlatoId = hamburguesa.Id, 
                        ItemId = tomate.Id, 
                        CantidadUsada = 2, 
                        UnidadMedida = "Unidad",
                        CreadoEn = DateTime.Now
                    },
                    new PlatoIngrediente { 
                        PlatoId = hamburguesa.Id, 
                        ItemId = cebolla.Id, 
                        CantidadUsada = 1, 
                        UnidadMedida = "Unidad",
                        CreadoEn = DateTime.Now
                    },
                    new PlatoIngrediente { 
                        PlatoId = hamburguesa.Id, 
                        ItemId = carne.Id, 
                        CantidadUsada = 0.25m, 
                        UnidadMedida = "Libra",
                        CreadoEn = DateTime.Now
                    },
                    new PlatoIngrediente { 
                        PlatoId = hamburguesa.Id, 
                        ItemId = pan.Id, 
                        CantidadUsada = 1, 
                        UnidadMedida = "Unidad",
                        CreadoEn = DateTime.Now
                    }
                };

                _context.PlatoIngredientes.AddRange(recetas);
                await _context.SaveChangesAsync();

                return Ok(new { 
                    message = $"✅ Receta creada: {hamburguesa.Nombre}",
                    plato = hamburguesa.Nombre,
                    totalIngredientes = recetas.Count,
                    receta = recetas.Select(r => new {
                        ingrediente = r.ItemId == lechuga.Id ? lechuga.Nombre :
                                     r.ItemId == tomate.Id ? tomate.Nombre :
                                     r.ItemId == cebolla.Id ? cebolla.Nombre :
                                     r.ItemId == carne.Id ? carne.Nombre : pan.Nombre,
                        cantidad = r.CantidadUsada,
                        unidad = r.UnidadMedida
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear recetas");
                return StatusCode(500, new { message = "❌ Error al crear recetas", error = ex.Message });
            }
        }

        // ========================================
        // 7. LIMPIAR INVENTARIO (PRODUCTOS)
        // ========================================
        [HttpDelete("inventario")]
        public async Task<IActionResult> LimpiarInventario()
        {
            try
            {
                var productos = await _context.Productos.ToListAsync();
                var cantidad = productos.Count;

                _context.Productos.RemoveRange(productos);
                await _context.SaveChangesAsync();

                return Ok(new { 
                    message = "✅ Inventario (Productos) limpiado exitosamente",
                    productosEliminados = cantidad
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al limpiar inventario");
                return StatusCode(500, new { message = "❌ Error al limpiar inventario", error = ex.Message });
            }
        }

        // ========================================
        // 8. LIMPIAR INGREDIENTES ⭐ NUEVO
        // ========================================
        [HttpDelete("inventario-items")]
        public async Task<IActionResult> LimpiarIngredientes()
        {
            try
            {
                // Primero eliminar recetas (por FK)
                var recetas = await _context.PlatoIngredientes.ToListAsync();
                _context.PlatoIngredientes.RemoveRange(recetas);

                // Luego eliminar ingredientes
                var ingredientes = await _context.InventarioItems.ToListAsync();
                var cantidad = ingredientes.Count;

                _context.InventarioItems.RemoveRange(ingredientes);
                await _context.SaveChangesAsync();

                return Ok(new { 
                    message = "✅ Ingredientes y recetas limpiados exitosamente",
                    ingredientesEliminados = cantidad,
                    recetasEliminadas = recetas.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al limpiar ingredientes");
                return StatusCode(500, new { message = "❌ Error al limpiar ingredientes", error = ex.Message });
            }
        }

        // ========================================
        // 9. LIMPIAR RECETAS ⭐ NUEVO
        // ========================================
        [HttpDelete("recetas-ingredientes")]
        public async Task<IActionResult> LimpiarRecetas()
        {
            try
            {
                var recetas = await _context.PlatoIngredientes.ToListAsync();
                var cantidad = recetas.Count;

                _context.PlatoIngredientes.RemoveRange(recetas);
                await _context.SaveChangesAsync();

                return Ok(new { 
                    message = "✅ Recetas limpiadas exitosamente",
                    recetasEliminadas = cantidad
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al limpiar recetas");
                return StatusCode(500, new { message = "❌ Error al limpiar recetas", error = ex.Message });
            }
        }
    }
}