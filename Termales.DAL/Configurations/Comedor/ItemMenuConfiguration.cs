using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Termales.Entities.Models.Comedor;

namespace Termales.DAL.Configurations.Comedor;

public class ItemMenuConfiguration : IEntityTypeConfiguration<ItemMenu>
{
    public void Configure(EntityTypeBuilder<ItemMenu> builder)
    {
        builder.ToTable("items_menu", "comedor");
        builder.HasKey(i => i.ItemMenuId);
        builder.Property(i => i.ItemMenuId).HasColumnName("item_menu_id").ValueGeneratedOnAdd();
        builder.Property(i => i.CategoriaMenuId).HasColumnName("categoria_menu_id");
        builder.Property(i => i.Nombre).HasColumnName("nombre").HasMaxLength(150).IsRequired();
        builder.Property(i => i.Descripcion).HasColumnName("descripcion").HasMaxLength(300);
        builder.Property(i => i.Precio).HasColumnName("precio").HasPrecision(10, 2);
        builder.Property(i => i.Activo).HasColumnName("activo").HasDefaultValue(true);

        builder.HasOne(i => i.Categoria)
               .WithMany(c => c.Items)
               .HasForeignKey(i => i.CategoriaMenuId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
