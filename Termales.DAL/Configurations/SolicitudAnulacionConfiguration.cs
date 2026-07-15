using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Termales.Entities.Models;

namespace Termales.DAL.Configurations;

public class SolicitudAnulacionConfiguration : IEntityTypeConfiguration<SolicitudAnulacion>
{
    public void Configure(EntityTypeBuilder<SolicitudAnulacion> builder)
    {
        builder.ToTable("solicitudes_anulacion");
        builder.HasKey(s => s.SolicitudAnulacionId);
        builder.Property(s => s.SolicitudAnulacionId).HasColumnName("solicitud_anulacion_id").ValueGeneratedOnAdd();
        builder.Property(s => s.ComprobanteId).HasColumnName("comprobante_id").IsRequired();
        builder.Property(s => s.Motivo).HasColumnName("motivo").HasMaxLength(500).IsRequired();
        builder.Property(s => s.SolicitadoPor).HasColumnName("solicitado_por").HasMaxLength(150).IsRequired();
        builder.Property(s => s.EstadoAnteriorComprobante).HasColumnName("estado_anterior_comprobante").HasMaxLength(50).IsRequired();
        builder.Property(s => s.FechaSolicitud).HasColumnName("fecha_solicitud");
        builder.Property(s => s.Estado).HasColumnName("estado").HasMaxLength(20).IsRequired();
        builder.Property(s => s.ResueltoPor).HasColumnName("resuelto_por").HasMaxLength(150);
        builder.Property(s => s.FechaResolucion).HasColumnName("fecha_resolucion");
        builder.Property(s => s.MotivoRechazo).HasColumnName("motivo_rechazo").HasMaxLength(500);
        builder.Property(s => s.NotaCreditoComprobanteId).HasColumnName("nota_credito_comprobante_id");

        builder.HasOne(s => s.Comprobante)
               .WithMany()
               .HasForeignKey(s => s.ComprobanteId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.NotaCreditoComprobante)
               .WithMany()
               .HasForeignKey(s => s.NotaCreditoComprobanteId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => s.ComprobanteId);
        builder.HasIndex(s => s.Estado);
    }
}
