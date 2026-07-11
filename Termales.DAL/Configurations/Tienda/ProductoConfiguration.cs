using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Termales.Entities.Models.Tienda;

namespace Termales.DAL.Configurations.Tienda;

public class ProductoConfiguration : IEntityTypeConfiguration<Producto>
{
    public void Configure(EntityTypeBuilder<Producto> builder)
    {
        builder.ToTable("productos", "tienda");
        builder.HasKey(p => p.ProductoId);
        builder.Property(p => p.ProductoId).HasColumnName("producto_id").ValueGeneratedOnAdd();
        builder.Property(p => p.Nombre).HasColumnName("nombre").HasMaxLength(200).IsRequired();
        builder.Property(p => p.Descripcion).HasColumnName("descripcion").HasMaxLength(500).HasDefaultValue("----");
        builder.Property(p => p.CodigoBarras).HasColumnName("codigo_barras").HasMaxLength(50);
        builder.Property(p => p.PrecioCompra).HasColumnName("precio_compra").HasPrecision(10, 2).HasDefaultValue(0m);
        builder.Property(p => p.Precio).HasColumnName("precio").HasPrecision(10, 2);
        builder.Property(p => p.Stock).HasColumnName("stock").HasDefaultValue(0);
        builder.Property(p => p.Activo).HasColumnName("activo").HasDefaultValue(true);
        builder.Property(p => p.FechaRegistro).HasColumnName("fecha_registro");

        builder.HasIndex(p => p.CodigoBarras).IsUnique().HasFilter("codigo_barras IS NOT NULL");
    }
}
