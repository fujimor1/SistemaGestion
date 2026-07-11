using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Termales.Entities.Models.Caja;

namespace Termales.DAL.Configurations.Caja;

public class AperturaCajaConfiguration : IEntityTypeConfiguration<AperturaCaja>
{
    public void Configure(EntityTypeBuilder<AperturaCaja> builder)
    {
        builder.ToTable("aperturas_caja", "caja");
        builder.HasKey(a => a.AperturaCajaId);
        builder.Property(a => a.AperturaCajaId).HasColumnName("apertura_caja_id");
        builder.Property(a => a.Fecha).HasColumnName("fecha");
        builder.Property(a => a.MontoInicial).HasColumnName("monto_inicial").HasPrecision(12, 2);
        builder.Property(a => a.Responsable).HasColumnName("responsable").HasMaxLength(150).IsRequired();
        builder.Property(a => a.Observaciones).HasColumnName("observaciones").HasMaxLength(300);
        builder.Property(a => a.FechaRegistro).HasColumnName("fecha_registro").HasDefaultValueSql("now()");
        builder.HasIndex(a => a.Fecha);
    }
}
