using megadeliciasapi.Data;
using megadeliciasapi.Models;
using Microsoft.EntityFrameworkCore;

namespace megadeliciasapi.Services
{
    public class InventarioService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<InventarioService> _logger;

        public InventarioService(ApplicationDbContext context, ILogger<InventarioService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> DescontarIngredientesPorOrden(int ordenId)
        {
            try
            {
                var orden = await _context.Ordenes
                    .Include(o => o.Detalles)
                    .ThenInclude(d => d.Plato)
                    .FirstOrDefaultAsync(o => o.Id == ordenId);

                if (orden == null)
                {
                    _logger.LogWarning("Orden {OrdenId} no encontrada", ordenId);
                    return false;
                }

                foreach (var detalle in orden.Detalles)
                {
                    var ingredientes = await _context.PlatoIngredientes
                        .Where(pi => pi.PlatoId == detalle.PlatoId)
                        .Include(pi => pi.InventarioItem)
                        .ToListAsync();

                    if (!ingredientes.Any())
                    {
                        _logger.LogWarning("Plato {PlatoNombre} no tiene ingredientes configurados", detalle.Plato?.Nombre);
                        continue;
                    }

                    // Verificar stock suficiente
                    foreach (var ingrediente in ingredientes)
                    {
                        if (ingrediente.InventarioItem == null)
                        {
                            _logger.LogWarning("Ingrediente {IngredienteId} no tiene InventarioItem asociado", ingrediente.Id);
                            continue;
                        }

                        var cantidadNecesaria = ingrediente.CantidadUsada * detalle.Cantidad;

                        if (ingrediente.InventarioItem.StockActual < (int)Math.Ceiling(cantidadNecesaria))
                        {
                            _logger.LogError(
                                "Stock insuficiente: {Ingrediente} necesita {Necesaria} {Unidad} pero solo hay {Disponible}",
                                ingrediente.InventarioItem.Nombre,
                                cantidadNecesaria,
                                ingrediente.UnidadMedida,
                                ingrediente.InventarioItem.StockActual
                            );
                            return false;
                        }
                    }

                    // Descontar ingredientes
                    foreach (var ingrediente in ingredientes)
                    {
                        if (ingrediente.InventarioItem == null) continue;

                        var cantidadNecesaria = ingrediente.CantidadUsada * detalle.Cantidad;

                        // ⭐ DESCONTAR CON CAST A INT (StockActual es int)
                        ingrediente.InventarioItem.StockActual -= (int)Math.Ceiling(cantidadNecesaria);

                        // Registrar movimiento
                        var movimiento = new InventarioMovimiento
                        {
                            ItemId = ingrediente.ItemId,
                            PlatoIngredienteId = ingrediente.Id,
                            Tipo = "CONSUMO_COCINA",
                            Cantidad = (int)Math.Ceiling(cantidadNecesaria),
                            CostoUnitario = ingrediente.InventarioItem.CostoUnitario,
                            Motivo = $"Orden #{ordenId} - {detalle.Plato?.Nombre ?? "Plato"} x{detalle.Cantidad}",
                            Fecha = DateTime.Now
                        };

                        _context.InventarioMovimientos.Add(movimiento);
                    }
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Ingredientes descontados exitosamente para orden {OrdenId}", ordenId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al descontar ingredientes de orden {OrdenId}", ordenId);
                return false;
            }
        }
    }
}