using System.ComponentModel.DataAnnotations;

namespace InFerreteria.Models
{
    public class CategoriaCreateVm
    {
        [Required, StringLength(150)]
        public string Nombre { get; set; } = "";

        [StringLength(10000)]
        public string? Descripcion { get; set; }

        public bool Activo { get; set; } = true;
    }
}
