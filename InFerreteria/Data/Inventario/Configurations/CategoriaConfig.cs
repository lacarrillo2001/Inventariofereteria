using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApp.Data.Inventario.Entities;

namespace WebApp.Data.Inventario.Configurations
{
    public class CategoriaConfig : IEntityTypeConfiguration<Categoria>
    {
        public void Configure(EntityTypeBuilder<Categoria> e)
        {
            e.ToTable("categoria");

            e.HasKey(p => p.Id);
            e.Property(p => p.Id).HasColumnName("id");

            e.Property(p => p.Nombre).HasColumnName("nombre")
                                      .HasMaxLength(100)
                                      .IsRequired();

            e.HasIndex(p => p.Nombre).IsUnique();

            e.Property(p => p.Descripcion).HasColumnName("descripcion").HasMaxLength(250);

            e.Property(p => p.Activo).HasColumnName("activo").HasDefaultValue(true);
            e.HasQueryFilter(x => x.Activo);

            e.Property(p => p.CreatedAt).HasColumnName("created_at");
            e.Property(p => p.UpdatedAt).HasColumnName("updated_at");

            
        }
    }
}
