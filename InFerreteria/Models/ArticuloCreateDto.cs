namespace Ferreteria.Web.Models
{
    public class ArticuloCreateDto
    {

        
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public int CategoriaId { get; set; }
        public int ProveedorId { get; set; }
        public decimal PrecioCompra { get; set; }
        public decimal PrecioVenta { get; set; }
        public int Stock { get; set; }
        public int StockMinimo { get; set; }
        public string? Descripcion { get; set; }
    }
}
