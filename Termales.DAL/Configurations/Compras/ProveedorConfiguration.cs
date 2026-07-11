using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Termales.Entities.Models.Compras;

namespace Termales.DAL.Configurations.Compras;

public class ProveedorConfiguration : IEntityTypeConfiguration<Proveedor>
{
    public void Configure(EntityTypeBuilder<Proveedor> builder)
    {
        builder.ToTable("proveedores", "compras");
        builder.HasKey(p => p.ProveedorId);
        builder.Property(p => p.ProveedorId).HasColumnName("proveedor_id").ValueGeneratedOnAdd();
        builder.Property(p => p.Ruc).HasColumnName("ruc").HasMaxLength(11).IsRequired();
        builder.Property(p => p.RazonSocial).HasColumnName("razon_social").HasMaxLength(200).IsRequired();
        builder.Property(p => p.NombreComercial).HasColumnName("nombre_comercial").HasMaxLength(200);
        builder.Property(p => p.Direccion).HasColumnName("direccion").HasMaxLength(300);
        builder.Property(p => p.Telefono).HasColumnName("telefono").HasMaxLength(15);
        builder.Property(p => p.Email).HasColumnName("email").HasMaxLength(150);
        builder.Property(p => p.Activo).HasColumnName("activo").HasDefaultValue(true);

        builder.HasIndex(p => p.Ruc).IsUnique();
    }
}
