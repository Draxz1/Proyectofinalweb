using System;
using System.ComponentModel.DataAnnotations;

namespace megadeliciasapi.DTOs
{
    public class InventarioDTos
    {
        public int Id { get; set; }
        public string? Codigo { get; set; }
        public string Nombre { get; set; }

        public int StockActual { get; set; }
        public int StockMinimo { get; set; }
        public decimal CostoUnitario { get; set; }

        public string UnidadMedida { get; set; }
        public bool Activo { get; set; }
        public DateTime CreadoEn { get; set; }
        
       public int CategoriaId { get; set; }

    }
}