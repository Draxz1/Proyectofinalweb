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

        public SeedController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. SEMBRAR ESTADOS (Requerido para flujo de cocina)
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

            return Ok(new { message = $"Se agregaron {estados.Count} estados exitosamente." });
        }

        // 2. SEMBRAR MENÚ (Requerido para Panel Mesero)
        [HttpPost("menu")]
        public async Task<IActionResult> SeedMenu()
        {
            if (await _context.Platos.AnyAsync())
            {
                return Ok(new { message = "El menú ya tiene datos. No se hizo nada." });
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

            return Ok(new { message = $"Se agregaron {menu.Count} platos al menú exitosamente." });
        }

        // 3. SEMBRAR MESAS (Requerido para crear órdenes sin error 500)
        [HttpPost("mesas")]
        public async Task<IActionResult> SeedMesas()
        {
            if (await _context.Mesas.AnyAsync())
            {
                return Ok(new { message = "Las mesas ya existen." });
            }

            var mesas = new List<Mesa>();
            // Creamos 10 mesas (M-1 a M-10)
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

            return Ok(new { message = $"Se agregaron {mesas.Count} mesas." });
        }
    }
}