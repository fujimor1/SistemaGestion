using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Termales.Entities.Models;

namespace Termales.DAL.Configurations;

public class EmpleadoConfiguration : IEntityTypeConfiguration<Empleado>
{
    public void Configure(EntityTypeBuilder<Empleado> builder)
    {
        builder.ToTable("empleados");
        builder.HasKey(e => e.EmpleadoId);
        builder.Property(e => e.EmpleadoId).HasColumnName("empleado_id").ValueGeneratedOnAdd();
        builder.Property(e => e.Nombres).HasColumnName("nombres").HasMaxLength(100).IsRequired();
        builder.Property(e => e.Apellidos).HasColumnName("apellidos").HasMaxLength(100).IsRequired();
        builder.Property(e => e.Dni).HasColumnName("dni").HasMaxLength(8).IsRequired();
        builder.Property(e => e.Cargo).HasColumnName("cargo").HasConversion<int>();
        builder.Property(e => e.Telefono).HasColumnName("telefono").HasMaxLength(15);
        builder.Property(e => e.Email).HasColumnName("email").HasMaxLength(150).IsRequired();
        builder.Property(e => e.PasswordHash).HasColumnName("password_hash").HasMaxLength(256).IsRequired();
        builder.Property(e => e.FechaContrato).HasColumnName("fecha_contrato");
        builder.Property(e => e.Activo).HasColumnName("activo").HasDefaultValue(true);
        builder.HasIndex(e => e.Email).IsUnique();
        builder.HasIndex(e => e.Dni).IsUnique();
    }
}
