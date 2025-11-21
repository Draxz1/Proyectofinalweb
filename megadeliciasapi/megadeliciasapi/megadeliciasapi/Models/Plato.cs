using System.ComponentModel.DataAnnotations.Schema;

namespace megadeliciasapi.Models
{
    public class Plato
    {
        public int Id { get; set; }
        public string Nombre { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal Precio { get; set; }
        
        public string Estado { get; set; } // "Activo", "Inactivo"
        public bool Disponible { get; set; }

        // 👇 ¡AQUÍ ESTÁ LO QUE FALTA!
        public string? Categoria { get; set; } 
        
        [Column(TypeName = "decimal(10, 2)")]
        public decimal CostoPreparacion { get; set; }
        
        public DateTime CreadoEn { get; set; } = DateTime.Now;

        // Relaciones
        public ICollection<DetalleOrden> DetallesOrden { get; set; }
        public ICollection<PlatoIngrediente> Ingredientes { get; set; }
    }
}