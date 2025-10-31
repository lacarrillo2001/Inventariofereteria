using System.ComponentModel.DataAnnotations;

namespace InFerreteria.Models
{
    public class ProveedorCreateVm
    {
        [Required, StringLength(150)] public string Nombre { get; set; } = "";
        [StringLength(150)] public string? Contacto { get; set; }
        [StringLength(20)] public string? Ruc { get; set; }
        [EmailAddress, StringLength(200)] public string? Correo { get; set; }
        [StringLength(300)] public string? Direccion { get; set; }
        public bool Activo { get; set; } = true;
    }
}
