using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Termales.Entities.Models.Comedor;

namespace Termales.DAL.Configurations.Comedor;

public class CategoriaMenuConfiguration : IEntityTypeConfiguration<CategoriaMenu>
{
    public void Configure(EntityTypeBuilder<CategoriaMenu> builder)
    {
        builder.ToTable("categorias_menu", "comedor");
        builder.HasKey(c => c.CategoriaMenuId);
        builder.Property(c => c.CategoriaMenuId).HasColumnName("categoria_menu_id").ValueGeneratedOnAdd();
        builder.Property(c => c.Nombre).HasColumnName("nombre").HasMaxLength(100).IsRequired();
        builder.Property(c => c.Activo).HasColumnName("activo").HasDefaultValue(true);
    }
}
