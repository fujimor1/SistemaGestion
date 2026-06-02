using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Termales.Entities.Models.Seguridad;

namespace Termales.DAL.Configurations.Seguridad;

public class RolConfiguration : IEntityTypeConfiguration<Rol>
{
    public void Configure(EntityTypeBuilder<Rol> builder)
    {
        builder.ToTable("roles", "seguridad");
        builder.HasKey(r => r.RolId);
        builder.Property(r => r.RolId).HasColumnName("rol_id").ValueGeneratedOnAdd();
        builder.Property(r => r.Nombre).HasColumnName("nombre").HasMaxLength(50).IsRequired();
        builder.Property(r => r.Descripcion).HasColumnName("descripcion").HasMaxLength(200);
        builder.Property(r => r.Activo).HasColumnName("activo").HasDefaultValue(true);

        builder.HasIndex(r => r.Nombre).IsUnique();

        builder.HasData(
            new Rol { RolId = 1, Nombre = "Administrador", Descripcion = "Acceso total al sistema", Activo = true },
            new Rol { RolId = 2, Nombre = "Cajero", Descripcion = "Gestión de reservas y pagos", Activo = true },
            new Rol { RolId = 3, Nombre = "Recepcionista", Descripcion = "Atención al cliente y reservas", Activo = true }
        );
    }
}
