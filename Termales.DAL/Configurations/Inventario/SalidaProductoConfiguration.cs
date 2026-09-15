using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Termales.Entities.Models.Inventario;

namespace Termales.DAL.Configurations.Inventario;

public class SalidaProductoConfiguration : IEntityTypeConfiguration<SalidaProducto>
{
    public void Configure(EntityTypeBuilder<SalidaProducto> builder)
    {
        builder.ToTable("salidas_producto", "inventario");
        builder.HasKey(s => s.SalidaProductoId);
        builder.Property(s => s.SalidaProductoId).HasColumnName("salida_producto_id");
        builder.Property(s => s.ProductoId).HasColumnName("producto_id");
        builder.Property(s => s.Cantidad).HasColumnName("cantidad");
        builder.Property(s => s.Fecha).HasColumnName("fecha").HasDefaultValueSql("now()");
        builder.Property(s => s.Observacion).HasColumnName("observacion").HasMaxLength(300);

        builder.HasOne(s => s.Producto)
            .WithMany()
            .HasForeignKey(s => s.ProductoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
