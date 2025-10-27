using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApp.Data.Inventario.Entities;

namespace WebApp.Data.Inventario.Configurations
{
    public class ArticuloConfig : IEntityTypeConfiguration<Articulo>
    {
        public void Configure(EntityTypeBuilder<Articulo> e)
        {
            e.ToTable("articulo");

            e.HasKey(p => p.Id);
            e.Property(p => p.Id).HasColumnName("id");

            e.Property(p => p.Codigo).HasColumnName("codigo")
                                     .HasMaxLength(50)
                                     .IsRequired();

            e.HasIndex(p => p.Codigo).IsUnique();

            e.Property(p => p.CodigoBarras).HasColumnName("codigo_barras").HasMaxLength(80);

            e.Property(p => p.Nombre).HasColumnName("nombre")
                                     .HasMaxLength(200)
                                     .IsRequired();

            e.Property(p => p.PrecioCompra).HasColumnName("precio_compra").HasPrecision(12, 2);
            e.Property(p => p.PrecioVenta).HasColumnName("precio_venta").HasPrecision(12, 2);

            e.Property(p => p.StockActual).HasColumnName("stock_actual");
            e.Property(p => p.StockMinimo).HasColumnName("stock_minimo");

            e.Property(p => p.CategoriaId).HasColumnName("categoria_id");
            e.Property(p => p.ProveedorId).HasColumnName("proveedor_id");

            e.HasOne(p => p.Categoria)
             .WithMany(c => c.Articulos)
             .HasForeignKey(p => p.CategoriaId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(p => p.Proveedor)
             .WithMany(c => c.Articulos)
             .HasForeignKey(p => p.ProveedorId)
             .OnDelete(DeleteBehavior.Restrict);

            e.Property(p => p.Activo).HasColumnName("activo").HasDefaultValue(true);
            e.HasQueryFilter(x => x.Activo);

            e.Property(p => p.CreatedAt).HasColumnName("created_at");
            e.Property(p => p.UpdatedAt).HasColumnName("updated_at");
        }
    }
}
