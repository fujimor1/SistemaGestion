using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Termales.Entities.Models.Comedor;

namespace Termales.DAL.Configurations.Comedor;

public class MesaConfiguration : IEntityTypeConfiguration<Mesa>
{
    public void Configure(EntityTypeBuilder<Mesa> builder)
    {
        builder.ToTable("mesas", "comedor");
        builder.HasKey(m => m.MesaId);
        builder.Property(m => m.MesaId).HasColumnName("mesa_id").ValueGeneratedOnAdd();
        builder.Property(m => m.Numero).HasColumnName("numero").IsRequired();
        builder.Property(m => m.Capacidad).HasColumnName("capacidad").IsRequired();
        builder.Property(m => m.Estado).HasColumnName("estado").HasConversion<int>();
        builder.Property(m => m.Activo).HasColumnName("activo").HasDefaultValue(true);

        builder.HasIndex(m => m.Numero).IsUnique();
    }
}
