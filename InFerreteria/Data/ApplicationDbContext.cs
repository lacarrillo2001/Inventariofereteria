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
using WebApp.Data.Inventario.Entities;

namespace WebApp.Data
{
    // 👇 Cambiamos DbContext -> IdentityDbContext para que ASP.NET Core Identity se configure solo
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<Categoria> Categorias => Set<Categoria>();
        public DbSet<Proveedor> Proveedores => Set<Proveedor>();
        public DbSet<Articulo> Articulos => Set<Articulo>();

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<WebApp.Data.Infra.Entities.ErrorLog> ErrorLogs => Set<WebApp.Data.Infra.Entities.ErrorLog>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ⚠️ Importante para que Identity cree sus tablas/índices por defecto
            base.OnModelCreating(modelBuilder);

            // Aplica TODAS las configuraciones de la asamblea (Configurations/*)
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            // Mapea ErrorLog sin usar configuración aparte
            modelBuilder.Entity<WebApp.Data.Infra.Entities.ErrorLog>(e =>
            {
                e.ToTable("error_log");
                e.Property(p => p.Id).HasColumnName("id");
                e.Property(p => p.Level).HasColumnName("level");
                e.Property(p => p.Message).HasColumnName("message");
                e.Property(p => p.StackTrace).HasColumnName("stack_trace");
                e.Property(p => p.Controller).HasColumnName("controller");
                e.Property(p => p.Action).HasColumnName("action");
                e.Property(p => p.UserName).HasColumnName("user_name");
                e.Property(p => p.Path).HasColumnName("path");
                e.Property(p => p.QueryString).HasColumnName("query_string");
                e.Property(p => p.FormJson).HasColumnName("form_json");
                e.Property(p => p.CreatedAt).HasColumnName("created_at");
            });
        }

        // Marca UpdatedAt automáticamente en insert/update
        public override int SaveChanges()
        {
            TouchTimestamps();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            TouchTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void TouchTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is Categoria || e.Entity is Proveedor || e.Entity is Articulo);

            var now = DateTime.UtcNow;
            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Property("CreatedAt").CurrentValue = now;
                    entry.Property("UpdatedAt").CurrentValue = now;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Property("UpdatedAt").CurrentValue = now;
                }
            }
        }
    }
}
