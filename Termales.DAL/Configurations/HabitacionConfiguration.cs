using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Termales.Entities.Models;

namespace Termales.DAL.Configurations;

public class HabitacionConfiguration : IEntityTypeConfiguration<Habitacion>
{
    public void Configure(EntityTypeBuilder<Habitacion> builder)
    {
        builder.ToTable("habitaciones");
        builder.HasKey(h => h.HabitacionId);
        builder.Property(h => h.HabitacionId).HasColumnName("habitacion_id").ValueGeneratedOnAdd();
        builder.Property(h => h.Nombre).HasColumnName("nombre").HasMaxLength(100).IsRequired();
        builder.Property(h => h.Descripcion).HasColumnName("descripcion").HasMaxLength(500);
        builder.Property(h => h.Capacidad).HasColumnName("capacidad");
        builder.Property(h => h.Precio).HasColumnName("precio").HasPrecision(10, 2).HasDefaultValue(0m);
        builder.Property(h => h.Ocupado).HasColumnName("ocupado").HasDefaultValue(false);
        builder.Property(h => h.Activo).HasColumnName("activo").HasDefaultValue(true);
        builder.Property(h => h.FechaCheckIn).HasColumnName("fecha_check_in");
        builder.Property(h => h.FechaCheckOut).HasColumnName("fecha_check_out");
    }
}
