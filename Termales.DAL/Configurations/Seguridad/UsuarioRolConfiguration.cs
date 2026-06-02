using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Termales.Entities.Models.Seguridad;

namespace Termales.DAL.Configurations.Seguridad;

public class UsuarioRolConfiguration : IEntityTypeConfiguration<UsuarioRol>
{
    public void Configure(EntityTypeBuilder<UsuarioRol> builder)
    {
        builder.ToTable("usuario_roles", "seguridad");
        builder.HasKey(ur => ur.UsuarioRolId);
        builder.Property(ur => ur.UsuarioRolId).HasColumnName("usuario_rol_id").ValueGeneratedOnAdd();
        builder.Property(ur => ur.UsuarioId).HasColumnName("usuario_id");
        builder.Property(ur => ur.RolId).HasColumnName("rol_id");
        builder.Property(ur => ur.FechaAsignacion).HasColumnName("fecha_asignacion");

        builder.HasIndex(ur => new { ur.UsuarioId, ur.RolId }).IsUnique();

        builder.HasOne(ur => ur.Usuario)
               .WithMany(u => u.UsuarioRoles)
               .HasForeignKey(ur => ur.UsuarioId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ur => ur.Rol)
               .WithMany(r => r.UsuarioRoles)
               .HasForeignKey(ur => ur.RolId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
