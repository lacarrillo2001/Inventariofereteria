using System.ComponentModel.DataAnnotations;

namespace WebApp.ViewModels;

public class ArticuloCreateVm
{
    [Required, StringLength(50)]
    public string Codigo { get; set; } = default!;

    [Required, StringLength(200)]
    public string Nombre { get; set; } = default!;

    [Range(0, double.MaxValue)]
    public decimal PrecioCompra { get; set; }

    [Range(0, double.MaxValue)]
    public decimal PrecioVenta { get; set; }

    [Range(0, int.MaxValue)]
    public int Stock { get; set; }

    [Range(0, int.MaxValue)]
    public int StockMinimo { get; set; }

    [Required]
    public int CategoriaId { get; set; }

    [Required]
    public int ProveedorId { get; set; }

    [StringLength(500)]
    public string? Descripcion { get; set; }

    public bool Activo { get; set; } = true;
}
