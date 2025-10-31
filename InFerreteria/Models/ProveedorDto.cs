namespace InFerreteria.Models
{
    public class ProveedorDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string? Contacto { get; set; }
        public string? Ruc { get; set; }
        public string? Correo { get; set; }
        public string? Direccion { get; set; }
        public bool Activo { get; set; }
    }
}
