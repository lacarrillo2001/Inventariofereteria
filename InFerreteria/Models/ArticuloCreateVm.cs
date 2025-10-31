using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace Ferreteria.Web.Models
{
    public class ArticuloCreateVm
    {
        // Datos del artículo
        [Required] public string Codigo { get; set; } = string.Empty;
        [Required] public string Nombre { get; set; } = string.Empty;
        [Range(1, int.MaxValue)] public int CategoriaId { get; set; }
        [Range(1, int.MaxValue)] public int ProveedorId { get; set; }
        [Range(0, double.MaxValue)] public decimal PrecioCompra { get; set; }
        [Range(0, double.MaxValue)] public decimal PrecioVenta { get; set; }
        [Range(0, int.MaxValue)] public int Stock { get; set; }
        [Range(0, int.MaxValue)] public int StockMinimo { get; set; }
        public string? Descripcion { get; set; }

        // Combos
        public List<SelectItemVm> Categorias { get; set; } = new();
        public List<SelectItemVm> Proveedores { get; set; } = new();
    }
}
