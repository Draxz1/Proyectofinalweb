using megadeliciasapi.Data;
using megadeliciasapi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task<(bool Exito, string Mensaje)> ProcesarConsumoOrden(int ordenId)
        {
            // Usamos transacción para asegurar consistencia
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                var orden = await _context.Ordenes
                    .Include(o => o.Detalles)
                    .FirstOrDefaultAsync(o => o.Id == ordenId);

                if (orden == null) return (false, "Orden no encontrada");

                var errores = new List<string>();

                foreach (var detalle in orden.Detalles)
                {
                    // Buscamos los ingredientes definidos para este plato
                    var ingredientes = await _context.PlatoIngredientes
                        .Include(pi => pi.InventarioItem)
                        .Where(pi => pi.PlatoId == detalle.PlatoId)
                        .ToListAsync();

                    if (!ingredientes.Any()) continue; 

                    foreach (var ingrediente in ingredientes)
                    {
                        if (ingrediente.InventarioItem == null) continue;

                        // Cálculo de cantidad total a descontar
                        decimal cantidadNecesaria = ingrediente.CantidadUsada * detalle.Cantidad;
                        int cantidadDescontar = (int)Math.Ceiling(cantidadNecesaria);

                        // 1. Validar Stock
                        if (ingrediente.InventarioItem.StockActual < cantidadDescontar)
                        {
                            errores.Add($"Falta stock: {ingrediente.InventarioItem.Nombre} (Req: {cantidadDescontar}, Disp: {ingrediente.InventarioItem.StockActual})");
                            continue;
                        }

                        // 2. Descontar del Stock Real
                        ingrediente.InventarioItem.StockActual -= cantidadDescontar;

                        // 3. Registrar el Movimiento para que salga en el panel
                        var movimiento = new InventarioMovimiento
                        {
                            ItemId = ingrediente.ItemId,
                            PlatoIngredienteId = ingrediente.Id,
                            Tipo = "CONSUMO_COCINA",
                            Cantidad = cantidadDescontar,
                            CostoUnitario = ingrediente.InventarioItem.CostoUnitario,
                            Motivo = $"Orden #{ordenId}",
                            Fecha = DateTime.Now
                        };

                        _context.InventarioMovimientos.Add(movimiento);
                    }
                }

                if (errores.Any())
                {
                    await transaction.RollbackAsync();
                    return (false, string.Join(" | ", errores));
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                
                return (true, "Inventario actualizado");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error descontando inventario orden {Id}", ordenId);
                return (false, "Error interno: " + ex.Message);
            }
        }
    }
}