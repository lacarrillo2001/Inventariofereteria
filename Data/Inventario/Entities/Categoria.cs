using System.ComponentModel.DataAnnotations;

namespace WebApp.Data.Inventario.Entities
{
    public class Categoria
    {
        public long Id { get; set; }

        [Required, StringLength(100)]
        public string Nombre { get; set; } = default!;

        [StringLength(250)]
        public string? Descripcion { get; set; }

        public bool Activo { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Articulo> Articulos { get; set; } = new List<Articulo>();
    }
}
