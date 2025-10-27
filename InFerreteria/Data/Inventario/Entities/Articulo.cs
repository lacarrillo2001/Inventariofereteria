//using System.ComponentModel.DataAnnotations;
//using Microsoft.AspNetCore.Mvc.ModelBinding.Validation; 
//namespace WebApp.Data.Inventario.Entities
//{
//    public class Articulo : IValidatableObject
//    {
//        public long Id { get; set; }

//        [Required, StringLength(50)]
//        public string Codigo { get; set; } = default!;

//        // Si tu BD actual TIENE esta columna, déjala; si no, puedes quitarla.
//        [StringLength(80)]
//        public string? CodigoBarras { get; set; }

//        [Required, StringLength(200)]
//        public string Nombre { get; set; } = default!;

//        public decimal PrecioCompra { get; set; }
//        public decimal PrecioVenta { get; set; }

//        public int StockActual { get; set; } = 0;
//        public int StockMinimo { get; set; } = 0;

//        public long CategoriaId { get; set; }
//        public Categoria Categoria { get; set; } = default!;

//        public long ProveedorId { get; set; }
//        public Proveedor Proveedor { get; set; } = default!;

//        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
//        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

//        // Coherencia de precios: Venta >= Compra
//        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
//        {
//            if (PrecioVenta < PrecioCompra)
//            {
//                yield return new ValidationResult(
//                    "El precio de venta debe ser mayor o igual al precio de compra.",
//                    new[] { nameof(PrecioVenta), nameof(PrecioCompra) }
//                );
//            }
//        }
//    }
//}

// Articulo.cs

using Microsoft.AspNetCore.Mvc.ModelBinding.Validation; // 👈
using System.ComponentModel.DataAnnotations;

namespace WebApp.Data.Inventario.Entities
{
    public class Articulo : IValidatableObject
    {
        public long Id { get; set; }

        [Required, StringLength(50)]
        public string Codigo { get; set; } = default!;

        [StringLength(80)]
        public string? CodigoBarras { get; set; }

        [Required, StringLength(200)]
        public string Nombre { get; set; } = default!;

        [Range(0, 999999999.99)]
        public decimal PrecioCompra { get; set; }

        [Range(0, 999999999.99)]
        public decimal PrecioVenta { get; set; }

        [Range(0, int.MaxValue)]
        public int StockActual { get; set; } = 0;

        [Range(0, int.MaxValue)]
        public int StockMinimo { get; set; } = 0;

        // FK requeridos (estos sí vienen del form)
        [Required]
        public long CategoriaId { get; set; }

        [Required]
        public long ProveedorId { get; set; }

        // Navegaciones: NO se validan en el POST y pueden venir null
        [ValidateNever]
        public Categoria? Categoria { get; set; }   // 👈 nullable + ValidateNever

        [ValidateNever]
        public Proveedor? Proveedor { get; set; }   // 👈 nullable + ValidateNever

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool Activo { get; set; } = true;

        public IEnumerable<ValidationResult> Validate(ValidationContext ctx)
        {
            if (PrecioVenta < PrecioCompra)
                yield return new ValidationResult(
                    "El precio de venta debe ser mayor o igual al precio de compra.",
                    new[] { nameof(PrecioVenta), nameof(PrecioCompra) });
        }
    }
}
