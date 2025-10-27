using System.ComponentModel.DataAnnotations;

namespace WebApp.Data.Inventario.Entities
{
    public class Proveedor
    {
        public long Id { get; set; }

        [Required, StringLength(150)]
        public string Nombre { get; set; } = default!;

        [StringLength(13)]
        public string? Ruc { get; set; }

        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(20)]
        public string? Telefono { get; set; }

        [StringLength(200)]
        public string? Direccion { get; set; }

        public bool Activo { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Articulo> Articulos { get; set; } = new List<Articulo>();
    }
}
