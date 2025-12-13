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
        // 0. SEMBRAR MÉTODOS DE PAGO (¡INDISPENSABLE!)
        // ========================================
        [HttpPost("metodos-pago")]
        public async Task<IActionResult> SeedMetodosPago()
        {
            try
            {
                if (await _context.MetodosPago.AnyAsync())
                {
                    return Ok(new { message = "⚠️ Los métodos de pago ya existen." });
                }

                var metodos = new List<MetodoPago>
                {
                    new MetodoPago { Nombre = "Efectivo", Activo = true },
                    new MetodoPago { Nombre = "Tarjeta", Activo = true }, // Para POS
                    new MetodoPago { Nombre = "Transferencia", Activo = true }
                };

                await _context.MetodosPago.AddRangeAsync(metodos);
                await _context.SaveChangesAsync();

                return Ok(new { message = "✅ Métodos de pago (Efectivo, Tarjeta, Transferencia) creados exitosamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al crear métodos de pago", error = ex.Message });
            }
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
                
                // ✅ IMPORTANTE: Nombres exactos para las alitas (coinciden con receta)
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
        // 4. SEMBRAR INVENTARIO (PRODUCTOS) - Panel General
        // ========================================
        [HttpPost("inventario")]
        public async Task<IActionResult> SeedInventario()
        {
            try
            {
                if (await _context.Productos.AnyAsync())
                {
                    return Ok(new { message = "⚠️ Ya existen productos en el inventario." });
                }

                var productos = new List<Producto>
                {
                    // CARNES
                    new Producto { Nombre = "Carne Molida Especial", Descripcion = "Carne molida de res premium", Categoria = "Carnes", PrecioUnitario = 85.00m, Stock = 5, StockMinimo = 2, UnidadMedida = "Libra", Activo = true },
                    new Producto { Nombre = "Pechuga de Pollo", Descripcion = "Pechuga de pollo fresca sin hueso", Categoria = "Carnes", PrecioUnitario = 60.00m, Stock = 8, StockMinimo = 3, UnidadMedida = "Libra", Activo = true },
                    new Producto { Nombre = "Costilla de Cerdo", Descripcion = "Costilla de cerdo con hueso", Categoria = "Carnes", PrecioUnitario = 95.00m, Stock = 2, StockMinimo = 1, UnidadMedida = "Libra", Activo = true },
                    // ... (Se mantiene igual)
                };

                _context.Productos.AddRange(productos);
                await _context.SaveChangesAsync();

                return Ok(new { message = "✅ Inventario inicial creado exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "❌ Error al crear inventario", error = ex.Message });
            }
        }

        // ========================================
        // 5. SEMBRAR INGREDIENTES REALISTAS (InventarioItems)
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

                var categoriaIngredientes = await _context.Categorias.FirstOrDefaultAsync(c => c.Nombre == "Ingredientes");
                
                if (categoriaIngredientes == null)
                {
                    categoriaIngredientes = new Categoria { Nombre = "Ingredientes" };
                    _context.Categorias.Add(categoriaIngredientes);
                    await _context.SaveChangesAsync();
                }

                var ingredientes = new List<InventarioItem>
                {
                    // === PROTEÍNAS ===
                    new InventarioItem { Nombre = "Alita de Pollo Marinada", StockActual = 200, StockMinimo = 24, CostoUnitario = 8m, UnidadMedida = "Unidad", CategoriaId = categoriaIngredientes.Id },
                    new InventarioItem { Nombre = "Carne Molida de Res", StockActual = 30, StockMinimo = 5, CostoUnitario = 120m, UnidadMedida = "Libra", CategoriaId = categoriaIngredientes.Id },
                    new InventarioItem { Nombre = "Chuleta de Cerdo", StockActual = 40, StockMinimo = 10, CostoUnitario = 45m, UnidadMedida = "Unidad", CategoriaId = categoriaIngredientes.Id },
                    new InventarioItem { Nombre = "Camarones Limpios", StockActual = 20, StockMinimo = 5, CostoUnitario = 180m, UnidadMedida = "Libra", CategoriaId = categoriaIngredientes.Id },
                    new InventarioItem { Nombre = "Huevo", StockActual = 150, StockMinimo = 30, CostoUnitario = 4m, UnidadMedida = "Unidad", CategoriaId = categoriaIngredientes.Id },

                    // === GUARNICIONES Y VERDURAS ===
                    new InventarioItem { Nombre = "Plátano Verde", StockActual = 60, StockMinimo = 15, CostoUnitario = 5m, UnidadMedida = "Unidad", CategoriaId = categoriaIngredientes.Id },
                    new InventarioItem { Nombre = "Papa (Para freír)", StockActual = 100, StockMinimo = 20, CostoUnitario = 10m, UnidadMedida = "Libra", CategoriaId = categoriaIngredientes.Id },
                    new InventarioItem { Nombre = "Lechuga Romana", StockActual = 50, StockMinimo = 10, CostoUnitario = 15m, UnidadMedida = "Unidad", CategoriaId = categoriaIngredientes.Id },
                    new InventarioItem { Nombre = "Tomate Fresco", StockActual = 100, StockMinimo = 20, CostoUnitario = 5m, UnidadMedida = "Unidad", CategoriaId = categoriaIngredientes.Id },
                    new InventarioItem { Nombre = "Cebolla Blanca", StockActual = 80, StockMinimo = 15, CostoUnitario = 4m, UnidadMedida = "Unidad", CategoriaId = categoriaIngredientes.Id },
                    new InventarioItem { Nombre = "Repollo", StockActual = 20, StockMinimo = 5, CostoUnitario = 20m, UnidadMedida = "Unidad", CategoriaId = categoriaIngredientes.Id },

                    // === PANES Y HARINAS ===
                    new InventarioItem { Nombre = "Pan de Hamburguesa", StockActual = 100, StockMinimo = 20, CostoUnitario = 6m, UnidadMedida = "Unidad", CategoriaId = categoriaIngredientes.Id },
                    new InventarioItem { Nombre = "Tortilla de Maíz", StockActual = 200, StockMinimo = 50, CostoUnitario = 1m, UnidadMedida = "Unidad", CategoriaId = categoriaIngredientes.Id },
                    new InventarioItem { Nombre = "Frijoles Rojos Cocidos", StockActual = 50, StockMinimo = 10, CostoUnitario = 15m, UnidadMedida = "Libra", CategoriaId = categoriaIngredientes.Id },

                    // === LÁCTEOS Y EXTRAS ===
                    new InventarioItem { Nombre = "Queso Amarillo (Slice)", StockActual = 100, StockMinimo = 20, CostoUnitario = 3m, UnidadMedida = "Unidad", CategoriaId = categoriaIngredientes.Id },
                    new InventarioItem { Nombre = "Queso Seco Rallado", StockActual = 15, StockMinimo = 3, CostoUnitario = 60m, UnidadMedida = "Libra", CategoriaId = categoriaIngredientes.Id },
                    new InventarioItem { Nombre = "Mantequilla Crema", StockActual = 20, StockMinimo = 5, CostoUnitario = 45m, UnidadMedida = "Libra", CategoriaId = categoriaIngredientes.Id }
                };

                _context.InventarioItems.AddRange(ingredientes);
                await _context.SaveChangesAsync();

                return Ok(new { 
                    message = "✅ Ingredientes realistas creados exitosamente",
                    totalIngredientes = ingredientes.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear ingredientes");
                return StatusCode(500, new { message = "❌ Error al crear ingredientes", error = ex.Message });
            }
        }

        // ========================================
        // 6. SEMBRAR RECETAS (PlatoIngrediente) CON ALITAS 6/12
        // ========================================
        [HttpPost("recetas-ingredientes")]
        public async Task<IActionResult> SeedRecetasIngredientes()
        {
            try
            {
                if (await _context.PlatoIngredientes.AnyAsync())
                {
                    return Ok(new { message = "⚠️ Las recetas ya existen. Limpia primero." });
                }

                // --- 1. CARGAR PLATOS ---
                var hamburguesa = await _context.Platos.FirstOrDefaultAsync(p => p.Nombre.Contains("Hamburguesa"));
                var alitas6 = await _context.Platos.FirstOrDefaultAsync(p => p.Nombre.Contains("Alitas de la casa (6)"));
                var alitas12 = await _context.Platos.FirstOrDefaultAsync(p => p.Nombre.Contains("Alitas de la casa (12)"));
                var chuleta = await _context.Platos.FirstOrDefaultAsync(p => p.Nombre.Contains("Chuleta con tajadas"));
                var desayuno = await _context.Platos.FirstOrDefaultAsync(p => p.Nombre.Contains("Catracho"));

                // --- 2. CARGAR INGREDIENTES ---
                // Proteínas
                var alitaCruda = await _context.InventarioItems.FirstOrDefaultAsync(i => i.Nombre == "Alita de Pollo Marinada");
                var carneMolida = await _context.InventarioItems.FirstOrDefaultAsync(i => i.Nombre == "Carne Molida de Res");
                var chuletaCruda = await _context.InventarioItems.FirstOrDefaultAsync(i => i.Nombre == "Chuleta de Cerdo");
                var huevo = await _context.InventarioItems.FirstOrDefaultAsync(i => i.Nombre == "Huevo");

                // Guarniciones
                var papa = await _context.InventarioItems.FirstOrDefaultAsync(i => i.Nombre.Contains("Papa"));
                var platano = await _context.InventarioItems.FirstOrDefaultAsync(i => i.Nombre == "Plátano Verde");
                var lechuga = await _context.InventarioItems.FirstOrDefaultAsync(i => i.Nombre.Contains("Lechuga"));
                var tomate = await _context.InventarioItems.FirstOrDefaultAsync(i => i.Nombre.Contains("Tomate"));
                var cebolla = await _context.InventarioItems.FirstOrDefaultAsync(i => i.Nombre.Contains("Cebolla"));
                var repollo = await _context.InventarioItems.FirstOrDefaultAsync(i => i.Nombre == "Repollo");
                var frijoles = await _context.InventarioItems.FirstOrDefaultAsync(i => i.Nombre.Contains("Frijoles"));
                
                // Panes / Harinas / Lacteos
                var panHamb = await _context.InventarioItems.FirstOrDefaultAsync(i => i.Nombre.Contains("Pan de Hamburguesa"));
                var tortilla = await _context.InventarioItems.FirstOrDefaultAsync(i => i.Nombre == "Tortilla de Maíz");
                var quesoSlice = await _context.InventarioItems.FirstOrDefaultAsync(i => i.Nombre.Contains("Queso Amarillo"));
                var quesoSeco = await _context.InventarioItems.FirstOrDefaultAsync(i => i.Nombre.Contains("Queso Seco"));
                var mantequilla = await _context.InventarioItems.FirstOrDefaultAsync(i => i.Nombre.Contains("Mantequilla"));

                var recetas = new List<PlatoIngrediente>();

                // --- RECETA 1: HAMBURGUESA (Con papas) ---
                if (hamburguesa != null && panHamb != null)
                {
                    recetas.Add(new PlatoIngrediente { PlatoId = hamburguesa.Id, ItemId = panHamb.Id, CantidadUsada = 1, UnidadMedida = "Unidad", CreadoEn = DateTime.Now });
                    if(carneMolida != null) recetas.Add(new PlatoIngrediente { PlatoId = hamburguesa.Id, ItemId = carneMolida.Id, CantidadUsada = 0.25m, UnidadMedida = "Libra", CreadoEn = DateTime.Now });
                    if(lechuga != null) recetas.Add(new PlatoIngrediente { PlatoId = hamburguesa.Id, ItemId = lechuga.Id, CantidadUsada = 1, UnidadMedida = "Unidad", CreadoEn = DateTime.Now });
                    if(tomate != null) recetas.Add(new PlatoIngrediente { PlatoId = hamburguesa.Id, ItemId = tomate.Id, CantidadUsada = 1, UnidadMedida = "Unidad", CreadoEn = DateTime.Now });
                    if(quesoSlice != null) recetas.Add(new PlatoIngrediente { PlatoId = hamburguesa.Id, ItemId = quesoSlice.Id, CantidadUsada = 1, UnidadMedida = "Unidad", CreadoEn = DateTime.Now });
                    if(papa != null) recetas.Add(new PlatoIngrediente { PlatoId = hamburguesa.Id, ItemId = papa.Id, CantidadUsada = 0.5m, UnidadMedida = "Libra", CreadoEn = DateTime.Now }); // Papas fritas
                }

                // --- RECETA 2: ALITAS (6 UNIDADES) ---
                if (alitas6 != null && alitaCruda != null)
                {
                    recetas.Add(new PlatoIngrediente { 
                        PlatoId = alitas6.Id, 
                        ItemId = alitaCruda.Id, 
                        CantidadUsada = 6, // ⭐ 6 Alitas
                        UnidadMedida = "Unidad",
                        CreadoEn = DateTime.Now
                    });
                    if(papa != null) recetas.Add(new PlatoIngrediente { PlatoId = alitas6.Id, ItemId = papa.Id, CantidadUsada = 0.5m, UnidadMedida = "Libra", CreadoEn = DateTime.Now });
                }

                // --- RECETA 3: ALITAS (12 UNIDADES) ---
                if (alitas12 != null && alitaCruda != null)
                {
                    recetas.Add(new PlatoIngrediente { 
                        PlatoId = alitas12.Id, 
                        ItemId = alitaCruda.Id, 
                        CantidadUsada = 12, // ⭐ 12 Alitas
                        UnidadMedida = "Unidad",
                        CreadoEn = DateTime.Now
                    });
                    if(papa != null) recetas.Add(new PlatoIngrediente { PlatoId = alitas12.Id, ItemId = papa.Id, CantidadUsada = 1.0m, UnidadMedida = "Libra", CreadoEn = DateTime.Now });
                }

                // --- RECETA 4: CHULETA CON TAJADAS ---
                if (chuleta != null && chuletaCruda != null)
                {
                    recetas.Add(new PlatoIngrediente { PlatoId = chuleta.Id, ItemId = chuletaCruda.Id, CantidadUsada = 1, UnidadMedida = "Unidad", CreadoEn = DateTime.Now });
                    if(platano != null) recetas.Add(new PlatoIngrediente { PlatoId = chuleta.Id, ItemId = platano.Id, CantidadUsada = 1, UnidadMedida = "Unidad", CreadoEn = DateTime.Now });
                    if(repollo != null) recetas.Add(new PlatoIngrediente { PlatoId = chuleta.Id, ItemId = repollo.Id, CantidadUsada = 0.25m, UnidadMedida = "Unidad", CreadoEn = DateTime.Now });
                }

                // --- RECETA 5: DESAYUNO CATRACHO ---
                if (desayuno != null && huevo != null)
                {
                    recetas.Add(new PlatoIngrediente { PlatoId = desayuno.Id, ItemId = huevo.Id, CantidadUsada = 2, UnidadMedida = "Unidad", CreadoEn = DateTime.Now });
                    if(frijoles != null) recetas.Add(new PlatoIngrediente { PlatoId = desayuno.Id, ItemId = frijoles.Id, CantidadUsada = 0.25m, UnidadMedida = "Libra", CreadoEn = DateTime.Now });
                    if(tortilla != null) recetas.Add(new PlatoIngrediente { PlatoId = desayuno.Id, ItemId = tortilla.Id, CantidadUsada = 2, UnidadMedida = "Unidad", CreadoEn = DateTime.Now });
                    if(quesoSeco != null) recetas.Add(new PlatoIngrediente { PlatoId = desayuno.Id, ItemId = quesoSeco.Id, CantidadUsada = 0.1m, UnidadMedida = "Libra", CreadoEn = DateTime.Now });
                    if(mantequilla != null) recetas.Add(new PlatoIngrediente { PlatoId = desayuno.Id, ItemId = mantequilla.Id, CantidadUsada = 0.1m, UnidadMedida = "Libra", CreadoEn = DateTime.Now });
                }

                _context.PlatoIngredientes.AddRange(recetas);
                await _context.SaveChangesAsync();

                return Ok(new { 
                    message = "✅ Recetas realistas configuradas correctamente.",
                    totalRecetas = recetas.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear recetas");
                return StatusCode(500, new { message = "❌ Error al crear recetas", error = ex.Message });
            }
        }

        // ========================================
        // MÉTODOS DE LIMPIEZA
        // ========================================

        [HttpDelete("inventario")]
        public async Task<IActionResult> LimpiarInventario()
        {
            try
            {
                var productos = await _context.Productos.ToListAsync();
                _context.Productos.RemoveRange(productos);
                await _context.SaveChangesAsync();
                return Ok(new { message = "✅ Inventario (Productos) limpiado exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "❌ Error", error = ex.Message });
            }
        }

        [HttpDelete("inventario-items")]
        public async Task<IActionResult> LimpiarIngredientes()
        {
            try
            {
                // Limpiar recetas primero por FK
                var recetas = await _context.PlatoIngredientes.ToListAsync();
                _context.PlatoIngredientes.RemoveRange(recetas);

                var ingredientes = await _context.InventarioItems.ToListAsync();
                _context.InventarioItems.RemoveRange(ingredientes);
                
                await _context.SaveChangesAsync();
                return Ok(new { message = "✅ Ingredientes y recetas limpiados." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "❌ Error", error = ex.Message });
            }
        }

        [HttpDelete("recetas-ingredientes")]
        public async Task<IActionResult> LimpiarRecetas()
        {
            try
            {
                var recetas = await _context.PlatoIngredientes.ToListAsync();
                _context.PlatoIngredientes.RemoveRange(recetas);
                await _context.SaveChangesAsync();
                return Ok(new { message = "✅ Recetas limpiadas." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "❌ Error", error = ex.Message });
            }
        }
        
        [HttpDelete("menu")]
        public async Task<IActionResult> LimpiarMenu()
        {
             try
            {
                var platos = await _context.Platos.ToListAsync();
                _context.Platos.RemoveRange(platos);
                await _context.SaveChangesAsync();
                return Ok(new { message = "✅ Menú limpiado." });
            }
            catch (Exception ex) { return StatusCode(500, new { message = "Error", error = ex.Message }); }
        }
    }
}