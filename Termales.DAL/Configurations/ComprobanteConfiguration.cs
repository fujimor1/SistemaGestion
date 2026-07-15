using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Termales.Entities.Enums;
using Termales.Entities.Models;

namespace Termales.DAL.Configurations;

public class ComprobanteConfiguration : IEntityTypeConfiguration<Comprobante>
{
    public void Configure(EntityTypeBuilder<Comprobante> builder)
    {
        builder.ToTable("comprobantes");
        builder.HasKey(c => c.ComprobanteId);
        builder.Property(c => c.ComprobanteId).HasColumnName("comprobante_id").ValueGeneratedOnAdd();
        builder.Property(c => c.Serie).HasColumnName("serie").HasMaxLength(10).IsRequired();
        builder.Property(c => c.Numero).HasColumnName("numero").IsRequired();
        builder.Property(c => c.TipoAmbiente).HasColumnName("tipo_ambiente").HasMaxLength(20).IsRequired();
        builder.Property(c => c.ReferenciaId).HasColumnName("referencia_id");
        builder.Property(c => c.ClienteDni).HasColumnName("cliente_dni").HasMaxLength(20);
        builder.Property(c => c.ClienteNombre).HasColumnName("cliente_nombre").HasMaxLength(200);
        builder.Property(c => c.Total).HasColumnName("total").HasPrecision(10, 2).IsRequired();
        builder.Property(c => c.EnlacePdf).HasColumnName("enlace_pdf").HasMaxLength(500).IsRequired();
        builder.Property(c => c.FechaEmision).HasColumnName("fecha_emision");
        builder.Property(c => c.ComprobanteOrigenId).HasColumnName("comprobante_origen_id");
        builder.Property(c => c.MotivoAnulacion).HasColumnName("motivo_anulacion").HasMaxLength(500);
        builder.Property(c => c.AutorizadoPor).HasColumnName("autorizado_por").HasMaxLength(150);
        builder.Property(c => c.CodigoMotivoNc).HasColumnName("codigo_motivo_nc").HasMaxLength(2);
        builder.Property(c => c.MetodoPago).HasColumnName("metodo_pago").HasConversion<int>().HasDefaultValue(MetodoPago.Efectivo);
        builder.Property(c => c.Cobrado).HasColumnName("cobrado").HasDefaultValue(true);
        builder.Property(c => c.FechaCobro).HasColumnName("fecha_cobro");
        builder.Property(c => c.ClienteId).HasColumnName("cliente_id");

        builder.HasOne(c => c.ComprobanteOrigen)
               .WithMany()
               .HasForeignKey(c => c.ComprobanteOrigenId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Cliente)
               .WithMany()
               .HasForeignKey(c => c.ClienteId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => new { c.Serie, c.Numero }).IsUnique();
    }
}
