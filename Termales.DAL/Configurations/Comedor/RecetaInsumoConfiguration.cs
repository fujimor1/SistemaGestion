using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Termales.Entities.Models.Comedor;

namespace Termales.DAL.Configurations.Comedor;

public class RecetaInsumoConfiguration : IEntityTypeConfiguration<RecetaInsumo>
{
    public void Configure(EntityTypeBuilder<RecetaInsumo> builder)
    {
        builder.ToTable("receta_insumos", "comedor");
        builder.HasKey(r => r.RecetaInsumoId);
        builder.Property(r => r.RecetaInsumoId).HasColumnName("receta_insumo_id").ValueGeneratedOnAdd();
        builder.Property(r => r.ItemMenuId).HasColumnName("item_menu_id");
        builder.Property(r => r.InsumoId).HasColumnName("insumo_id");
        builder.Property(r => r.Cantidad).HasColumnName("cantidad").HasPrecision(12, 3);

        builder.HasOne(r => r.ItemMenu)
               .WithMany(i => i.Receta)
               .HasForeignKey(r => r.ItemMenuId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Insumo)
               .WithMany()
               .HasForeignKey(r => r.InsumoId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
