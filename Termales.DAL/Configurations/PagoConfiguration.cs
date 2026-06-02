using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Termales.Entities.Models;

namespace Termales.DAL.Configurations;

public class PagoConfiguration : IEntityTypeConfiguration<Pago>
{
    public void Configure(EntityTypeBuilder<Pago> builder)
    {
        builder.ToTable("pagos");
        builder.HasKey(p => p.PagoId);
        builder.Property(p => p.PagoId).HasColumnName("pago_id").ValueGeneratedOnAdd();
        builder.Property(p => p.ReservaId).HasColumnName("reserva_id");
        builder.Property(p => p.Monto).HasColumnName("monto").HasColumnType("decimal(10,2)");
        builder.Property(p => p.TipoPago).HasColumnName("tipo_pago").HasConversion<int>();
        builder.Property(p => p.FechaPago).HasColumnName("fecha_pago");
        builder.Property(p => p.NumeroComprobante).HasColumnName("numero_comprobante").HasMaxLength(50);
        builder.Property(p => p.Observaciones).HasColumnName("observaciones").HasMaxLength(500);
    }
}
