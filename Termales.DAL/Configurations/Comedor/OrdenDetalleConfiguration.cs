using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Termales.Entities.Models.Comedor;

namespace Termales.DAL.Configurations.Comedor;

public class OrdenDetalleConfiguration : IEntityTypeConfiguration<OrdenDetalle>
{
    public void Configure(EntityTypeBuilder<OrdenDetalle> builder)
    {
        builder.ToTable("orden_detalles", "comedor", t => t.HasCheckConstraint(
            "CK_orden_detalles_item_o_producto",
            "(item_menu_id IS NOT NULL AND producto_id IS NULL) OR (item_menu_id IS NULL AND producto_id IS NOT NULL)"));
        builder.HasKey(d => d.OrdenDetalleId);
        builder.Property(d => d.OrdenDetalleId).HasColumnName("orden_detalle_id").ValueGeneratedOnAdd();
        builder.Property(d => d.OrdenId).HasColumnName("orden_id");
        builder.Property(d => d.ItemMenuId).HasColumnName("item_menu_id");
        builder.Property(d => d.ProductoId).HasColumnName("producto_id");
        builder.Property(d => d.Cantidad).HasColumnName("cantidad");
        builder.Property(d => d.PrecioUnitario).HasColumnName("precio_unitario").HasPrecision(10, 2);
        builder.Property(d => d.Estado).HasColumnName("estado").HasConversion<int>();
        builder.Property(d => d.Observaciones).HasColumnName("observaciones").HasMaxLength(300);
        builder.Property(d => d.ComprobanteId).HasColumnName("comprobante_id");
        builder.Ignore(d => d.Subtotal);

        builder.HasOne(d => d.Orden)
               .WithMany(o => o.Detalles)
               .HasForeignKey(d => d.OrdenId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.Comprobante)
               .WithMany()
               .HasForeignKey(d => d.ComprobanteId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(d => d.ItemMenu)
               .WithMany(i => i.OrdenDetalles)
               .HasForeignKey(d => d.ItemMenuId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.Producto)
               .WithMany()
               .HasForeignKey(d => d.ProductoId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
