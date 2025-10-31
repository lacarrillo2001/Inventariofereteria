using Ferreteria.Web.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InFerreteria.Models
{
    public class ArticuloEditVm
    {
        
        public int Id { get; set; }
        [Required] public string Codigo { get; set; } = "";
        [Required] public string Nombre { get; set; } = "";
        [Range(1, int.MaxValue)] public int CategoriaId { get; set; }
        [Range(1, int.MaxValue)] public int ProveedorId { get; set; }
        [Range(0, double.MaxValue)] public decimal PrecioCompra { get; set; }
        [Range(0, double.MaxValue)] public decimal PrecioVenta { get; set; }
        [Range(0, int.MaxValue)] public int Stock { get; set; }
        [Range(0, int.MaxValue)] public int StockMinimo { get; set; }
        public string? Descripcion { get; set; }
        public bool Activo { get; set; }

        public List<SelectItemVm> Categorias { get; set; } = new();
        public List<SelectItemVm> Proveedores { get; set; } = new();
    }
}
