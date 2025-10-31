//using WebApp.Data.Inventario;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
//using Microsoft.AspNetCore.Http;
//using System.Security.Claims;

//namespace WebApp.Data
//{
//    public class ApplicationDbContext : IdentityDbContext
//    {
//        private readonly IHttpContextAccessor? _http;

//        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor? http = null)
//            : base(options)
//        {
//            _http = http;
//        }

//         Inventario
//        public DbSet<Categoria> Categorias => Set<Categoria>();
//        public DbSet<Proveedor> Proveedores => Set<Proveedor>();
//        public DbSet<Articulo> Articulos => Set<Articulo>();
//        public DbSet<Existencia> Existencias => Set<Existencia>();

//        protected override void OnModelCreating(ModelBuilder b)
//        {
//            base.OnModelCreating(b);

//             Articulo
//            b.Entity<Articulo>(e =>
//            {
//                e.HasIndex(x => x.Codigo).IsUnique();
//                e.Property(x => x.PrecioCompra).HasPrecision(12, 2);
//                e.Property(x => x.PrecioVenta).HasPrecision(12, 2);


//            });

//             Existencia (1–1 por Articulo)
//            b.Entity<Existencia>(e =>
//            {
//                e.HasIndex(x => x.ArticuloId).IsUnique();
//                e.Property(x => x.CantidadDisponible).HasPrecision(18, 4);
//            });
//        }

//        public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
//        {
//             Auditoría básica para Articulo
//            var user = _http?.HttpContext?.User?.FindFirstValue(ClaimTypes.Name)
//                       ?? _http?.HttpContext?.User?.Identity?.Name;
//            var now = DateTime.UtcNow;

//            foreach (var e in ChangeTracker.Entries<Articulo>())
//            {
//                if (e.State == EntityState.Added)
//                {
//                    e.Entity.CreatedAt = now;
//                    e.Entity.CreatedBy = user;
//                }
//                else if (e.State == EntityState.Modified)
//                {
//                    e.Entity.UpdatedAt = now;
//                    e.Entity.UpdatedBy = user;
//                }
//            }

//            return await base.SaveChangesAsync(ct);
//        }
//    }
//}
using Microsoft.AspNetCore.Identity.EntityFrameworkCore; // 👈 Identity
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;                          // (opcional si luego auditas por usuario)
using System.Threading;
using System.Threading.Tasks;


namespace WebApp.Data
{
    // 👇 Cambiamos DbContext -> IdentityDbContext para que ASP.NET Core Identity se configure solo
    public class ApplicationDbContext : IdentityDbContext
    {
     

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        


    }
}
