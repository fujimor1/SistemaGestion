using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Termales.Entities.Enums;
using Termales.Entities.Models;

namespace Termales.DAL.Configurations;

public class ComprobanteSunatConfiguration : IEntityTypeConfiguration<ComprobanteSunat>
{
    public void Configure(EntityTypeBuilder<ComprobanteSunat> builder)
    {
        builder.ToTable("comprobantes_sunat");
        builder.HasKey(c => c.ComprobanteId);
        builder.Property(c => c.ComprobanteId).HasColumnName("comprobante_id").ValueGeneratedNever();

        builder.Property(c => c.XmlFirmado).HasColumnName("xml_firmado").IsRequired();
        builder.Property(c => c.HashDigestValue).HasColumnName("hash_digest_value").HasMaxLength(100);

        builder.Property(c => c.CdrXml).HasColumnName("cdr_xml");
        builder.Property(c => c.CdrCodigoRespuesta).HasColumnName("cdr_codigo_respuesta");
        builder.Property(c => c.CdrDescripcion).HasColumnName("cdr_descripcion").HasMaxLength(500);
        builder.Property(c => c.ObservacionesSunat).HasColumnName("observaciones_sunat");

        builder.Property(c => c.Estado).HasColumnName("estado").HasConversion<int>().HasDefaultValue(EstadoEnvioSunat.Pendiente);
        builder.Property(c => c.IntentosEnvio).HasColumnName("intentos_envio").HasDefaultValue(0);

        builder.Property(c => c.FechaLimiteEnvio).HasColumnName("fecha_limite_envio");
        builder.Property(c => c.FechaEnvioSunat).HasColumnName("fecha_envio_sunat");
        builder.Property(c => c.TicketResumen).HasColumnName("ticket_resumen").HasMaxLength(50);

        builder.HasOne(c => c.Comprobante)
               .WithOne()
               .HasForeignKey<ComprobanteSunat>(c => c.ComprobanteId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
