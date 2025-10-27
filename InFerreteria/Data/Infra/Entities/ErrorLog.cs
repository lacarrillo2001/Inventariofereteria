using System.ComponentModel.DataAnnotations;

namespace WebApp.Data.Infra.Entities
{
    public class ErrorLog
    {
        public long Id { get; set; }

        [StringLength(20)]
        public string Level { get; set; } = "Error"; // Error | Warning | Info

        [Required]
        public string Message { get; set; } = default!;

        public string? StackTrace { get; set; }

        [StringLength(100)]
        public string? Controller { get; set; }

        [StringLength(100)]
        public string? Action { get; set; }

        [StringLength(150)]
        public string? UserName { get; set; }

        [StringLength(400)]
        public string? Path { get; set; }

        public string? QueryString { get; set; }
        public string? FormJson { get; set; }  // guarda ModelState/Request.Form serializado

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
