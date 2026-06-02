using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Termales.Entities.Models;

namespace Termales.DAL.Configurations;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("clientes");
        builder.HasKey(c => c.ClienteId);
        builder.Property(c => c.ClienteId).HasColumnName("cliente_id").ValueGeneratedOnAdd();
        builder.Property(c => c.Nombres).HasColumnName("nombres").HasMaxLength(100).IsRequired();
        builder.Property(c => c.Apellidos).HasColumnName("apellidos").HasMaxLength(100).IsRequired();
        builder.Property(c => c.Dni).HasColumnName("dni").HasMaxLength(8).IsRequired();
        builder.Property(c => c.Telefono).HasColumnName("telefono").HasMaxLength(15);
        builder.Property(c => c.Email).HasColumnName("email").HasMaxLength(150);
        builder.Property(c => c.Direccion).HasColumnName("direccion").HasMaxLength(250);
        builder.Property(c => c.FechaRegistro).HasColumnName("fecha_registro");
        builder.Property(c => c.Activo).HasColumnName("activo").HasDefaultValue(true);
        builder.HasIndex(c => c.Dni).IsUnique();
    }
}
