using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Termales.Entities.Models.Caja;

namespace Termales.DAL.Configurations.Caja;

public class CierreCajaConfiguration : IEntityTypeConfiguration<CierreCaja>
{
    public void Configure(EntityTypeBuilder<CierreCaja> builder)
    {
        builder.ToTable("cierres_caja", "caja");
        builder.HasKey(c => c.CierreCajaId);
        builder.Property(c => c.CierreCajaId).HasColumnName("cierre_caja_id");
        builder.Property(c => c.Fecha).HasColumnName("fecha");
        builder.Property(c => c.TotalSistema).HasColumnName("total_sistema").HasPrecision(12, 2);
        builder.Property(c => c.EfectivoSistema).HasColumnName("efectivo_sistema").HasPrecision(12, 2).HasDefaultValue(0m);
        builder.Property(c => c.YapeSistema).HasColumnName("yape_sistema").HasPrecision(12, 2).HasDefaultValue(0m);
        builder.Property(c => c.EfectivoFisico).HasColumnName("efectivo_fisico").HasPrecision(12, 2);
        builder.Property(c => c.YapeFisico).HasColumnName("yape_fisico").HasPrecision(12, 2);
        builder.Property(c => c.TransferenciaFisico).HasColumnName("transferencia_fisico").HasPrecision(12, 2);
        builder.Property(c => c.TotalEgresos).HasColumnName("total_egresos").HasPrecision(12, 2);
        builder.Property(c => c.MontoApertura).HasColumnName("monto_apertura").HasPrecision(12, 2);
        builder.Property(c => c.Diferencia).HasColumnName("diferencia").HasPrecision(12, 2);
        builder.Property(c => c.Observaciones).HasColumnName("observaciones").HasMaxLength(500);
        builder.Property(c => c.EncargadoCierre).HasColumnName("encargado_cierre").HasMaxLength(150);
        builder.Property(c => c.FechaRegistro).HasColumnName("fecha_registro").HasDefaultValueSql("now()");
        builder.HasIndex(c => c.Fecha).IsUnique();
    }
}
