using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Termales.Entities.Models.Inventario;

namespace Termales.DAL.Configurations.Inventario;

public class EntradaProductoConfiguration : IEntityTypeConfiguration<EntradaProducto>
{
    public void Configure(EntityTypeBuilder<EntradaProducto> builder)
    {
        builder.ToTable("entradas_producto", "inventario");
        builder.HasKey(e => e.EntradaProductoId);
        builder.Property(e => e.EntradaProductoId).HasColumnName("entrada_producto_id").ValueGeneratedOnAdd();
        builder.Property(e => e.ProductoId).HasColumnName("producto_id");
        builder.Property(e => e.Cantidad).HasColumnName("cantidad");
        builder.Property(e => e.PrecioUnitario).HasColumnName("precio_unitario").HasPrecision(10, 2);
        builder.Property(e => e.Total).HasColumnName("total").HasPrecision(12, 2);
        builder.Property(e => e.Fecha).HasColumnName("fecha");
        builder.Property(e => e.Observacion).HasColumnName("observacion").HasMaxLength(400);
        builder.Property(e => e.CompraId).HasColumnName("compra_id");

        builder.HasOne(e => e.Producto)
               .WithMany()
               .HasForeignKey(e => e.ProductoId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Compra)
               .WithMany()
               .HasForeignKey(e => e.CompraId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
