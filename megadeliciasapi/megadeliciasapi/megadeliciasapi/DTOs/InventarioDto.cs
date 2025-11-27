using System;
using System.Collections.Generic;

namespace megadeliciasapi.DTOs
{
    // DTO usado para listar/mostrar items de inventario
    public class InventarioItemDto
    {
        public int Id { get; set; }
        public string? Codigo { get; set; }
        public string Nombre { get; set; } = null!;

        public int StockActual { get; set; }
        public int StockMinimo { get; set; }
        public decimal CostoUnitario { get; set; }

        // Valor calculado (no se mapea a BD)
        public decimal ValorTotal => StockActual * CostoUnitario;

        public string UnidadMedida { get; set; } = null!;
        public bool Activo { get; set; }
        public DateTime CreadoEn { get; set; }

        public int CategoriaId { get; set; }
        public string? Categoria { get; set; }
    }

    // DTO para el formulario de registrar movimiento (entrada/salida)
    public class RegistrarMovimientoDto
    {
        public int ItemId { get; set; }
        public string Tipo { get; set; } = null!; // "ENTRADA" o "SALIDA"
        public int Cantidad { get; set; }

        // Opcional: en entradas puede venir el nuevo costo
        public decimal? CostoUnitario { get; set; }

        public string? Motivo { get; set; }
    }

    // DTO que responde si un plato es factible de preparar
    public class DisponibilidadPlatoDto
    {
        public int PlatoId { get; set; }
        public bool EstaDisponible { get; set; }
        public List<string> IngredientesFaltantes { get; set; } = new List<string>();
    }

    // DTO para mostrar movimientos en la UI
    public class MovimientoDto
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string ItemNombre { get; set; } = null!;
        public string Tipo { get; set; } = null!;
        public int Cantidad { get; set; }
        public decimal CostoUnitario { get; set; }
        public string Motivo { get; set; } = null!;
    }

    // DTO para crear/editar un item (campos opcionales donde aplica)
    public class CrearItemDto
    {
        public string? Codigo { get; set; }
        public string Nombre { get; set; } = null!;
        public int CategoriaId { get; set; }
        public string UnidadMedida { get; set; } = null!;
        public int StockMinimo { get; set; }

        // Opcionales al crear (si no vienen, el controlador puede asignar 0/true)
        public int? StockActual { get; set; }
        public decimal? CostoUnitario { get; set; }
        public bool? Activo { get; set; }
    }
}
