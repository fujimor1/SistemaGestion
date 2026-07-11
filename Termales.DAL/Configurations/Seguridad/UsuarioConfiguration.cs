using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Termales.Entities.Models.Seguridad;

namespace Termales.DAL.Configurations.Seguridad;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("usuarios", "seguridad");
        builder.HasKey(u => u.UsuarioId);
        builder.Property(u => u.UsuarioId).HasColumnName("usuario_id").ValueGeneratedOnAdd();
        builder.Property(u => u.Email).HasColumnName("email").HasMaxLength(150).IsRequired();
        builder.Property(u => u.PasswordHash).HasColumnName("password_hash").HasMaxLength(256).IsRequired();
        builder.Property(u => u.RolId).HasColumnName("rol_id");
        builder.Property(u => u.Activo).HasColumnName("activo").HasDefaultValue(true);
        builder.Property(u => u.FechaCreacion).HasColumnName("fecha_creacion");
        builder.Property(u => u.EmpleadoId).HasColumnName("empleado_id");

        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.EmpleadoId).IsUnique();

        builder.HasOne(u => u.Rol)
               .WithMany(r => r.Usuarios)
               .HasForeignKey(u => u.RolId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.Empleado)
               .WithOne()
               .HasForeignKey<Usuario>(u => u.EmpleadoId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
