using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Termales.Entities.Models;

namespace Termales.DAL.Configurations;

public class ComprobanteDetalleConfiguration : IEntityTypeConfiguration<ComprobanteDetalle>
{
    public void Configure(EntityTypeBuilder<ComprobanteDetalle> builder)
    {
        builder.ToTable("comprobante_detalles");
        builder.HasKey(d => d.ComprobanteDetalleId);
        builder.Property(d => d.ComprobanteDetalleId).HasColumnName("comprobante_detalle_id").ValueGeneratedOnAdd();
        builder.Property(d => d.ComprobanteId).HasColumnName("comprobante_id");
        builder.Property(d => d.Descripcion).HasColumnName("descripcion").HasMaxLength(300).IsRequired();
        builder.Property(d => d.Cantidad).HasColumnName("cantidad");
        builder.Property(d => d.PrecioUnitario).HasColumnName("precio_unitario").HasPrecision(10, 2);
        builder.Property(d => d.Subtotal).HasColumnName("subtotal").HasPrecision(10, 2);

        builder.HasOne(d => d.Comprobante)
               .WithMany(c => c.Detalles)
               .HasForeignKey(d => d.ComprobanteId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
