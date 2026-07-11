using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Termales.Entities.Models.Compras;

namespace Termales.DAL.Configurations.Compras;

public class DetalleCompraConfiguration : IEntityTypeConfiguration<DetalleCompra>
{
    public void Configure(EntityTypeBuilder<DetalleCompra> builder)
    {
        builder.ToTable("detalle_compras", "compras", t => t.HasCheckConstraint(
            "CK_detalle_compras_insumo_o_producto",
            "(insumo_id IS NOT NULL AND producto_id IS NULL) OR (insumo_id IS NULL AND producto_id IS NOT NULL)"));
        builder.HasKey(d => d.DetalleCompraId);
        builder.Property(d => d.DetalleCompraId).HasColumnName("detalle_compra_id").ValueGeneratedOnAdd();
        builder.Property(d => d.CompraId).HasColumnName("compra_id");
        builder.Property(d => d.TipoItem).HasColumnName("tipo_item").HasMaxLength(20).IsRequired();
        builder.Property(d => d.InsumoId).HasColumnName("insumo_id");
        builder.Property(d => d.ProductoId).HasColumnName("producto_id");
        builder.Property(d => d.Cantidad).HasColumnName("cantidad").HasPrecision(12, 3);
        builder.Property(d => d.PrecioUnitario).HasColumnName("precio_unitario").HasPrecision(10, 2);
        builder.Property(d => d.Total).HasColumnName("total").HasPrecision(10, 2);

        builder.HasOne(d => d.Compra)
               .WithMany(c => c.Detalles)
               .HasForeignKey(d => d.CompraId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.Insumo)
               .WithMany()
               .HasForeignKey(d => d.InsumoId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.Producto)
               .WithMany()
               .HasForeignKey(d => d.ProductoId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
