using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Termales.Entities.Models;

namespace Termales.DAL.Configurations;

public class HabitacionItemConfiguration : IEntityTypeConfiguration<HabitacionItem>
{
    public void Configure(EntityTypeBuilder<HabitacionItem> builder)
    {
        builder.ToTable("habitacion_items");
        builder.HasKey(i => i.HabitacionItemId);
        builder.Property(i => i.HabitacionItemId).HasColumnName("habitacion_item_id").ValueGeneratedOnAdd();
        builder.Property(i => i.HabitacionId).HasColumnName("habitacion_id");
        builder.Property(i => i.Nombre).HasColumnName("nombre").HasMaxLength(100).IsRequired();
        builder.Property(i => i.Cantidad).HasColumnName("cantidad").HasDefaultValue(1);

        builder.HasOne(i => i.Habitacion)
               .WithMany(h => h.Items)
               .HasForeignKey(i => i.HabitacionId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
