namespace InFerreteria.Models
{
    public class ArticuloDto
    {
        public int Id { get; set; }              // si el servicio lo devuelve
        public string Codigo { get; set; } = "";
        public string Nombre { get; set; } = "";
        public int CategoriaId { get; set; }
        public int ProveedorId { get; set; }
        public decimal PrecioCompra { get; set; }
        public decimal PrecioVenta { get; set; }
        public int Stock { get; set; }
        public int StockMinimo { get; set; }
        public string? Descripcion { get; set; }
        public bool Activo { get; set; }
    }
}
