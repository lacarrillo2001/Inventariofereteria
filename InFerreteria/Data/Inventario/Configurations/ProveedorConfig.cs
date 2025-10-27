using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApp.Data.Inventario.Entities;

namespace WebApp.Data.Inventario.Configurations
{
    public class ProveedorConfig : IEntityTypeConfiguration<Proveedor>
    {
        public void Configure(EntityTypeBuilder<Proveedor> e)
        {
            e.ToTable("proveedor");

            e.HasKey(p => p.Id);
            e.Property(p => p.Id).HasColumnName("id");

            e.Property(p => p.Nombre).HasColumnName("nombre")
                                      .HasMaxLength(150)
                                      .IsRequired();

            e.HasIndex(p => p.Nombre).IsUnique(false);

            e.Property(p => p.Ruc).HasColumnName("ruc").HasMaxLength(13);
            e.Property(p => p.Email).HasColumnName("email").HasMaxLength(100);
            e.Property(p => p.Telefono).HasColumnName("telefono").HasMaxLength(20);
            e.Property(p => p.Direccion).HasColumnName("direccion").HasMaxLength(200);

            e.Property(p => p.Activo).HasColumnName("activo").HasDefaultValue(true);
            e.HasQueryFilter(x => x.Activo);

            e.Property(p => p.CreatedAt).HasColumnName("created_at");
            e.Property(p => p.UpdatedAt).HasColumnName("updated_at");
        }
    }
}
