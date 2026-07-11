using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Termales.Entities.Models.Inventario;

namespace Termales.DAL.Configurations.Inventario;

public class InsumoConfiguration : IEntityTypeConfiguration<Insumo>
{
    public void Configure(EntityTypeBuilder<Insumo> builder)
    {
        builder.ToTable("insumos", "inventario");
        builder.HasKey(i => i.InsumoId);
        builder.Property(i => i.InsumoId).HasColumnName("insumo_id").ValueGeneratedOnAdd();
        builder.Property(i => i.Nombre).HasColumnName("nombre").HasMaxLength(200).IsRequired();
        builder.Property(i => i.TipoAmbiente).HasColumnName("tipo_ambiente").HasMaxLength(20).IsRequired();
        builder.Property(i => i.TipoArticulo).HasColumnName("tipo_articulo").HasMaxLength(20).HasDefaultValue("insumo");
        builder.Property(i => i.Unidad).HasColumnName("unidad").HasMaxLength(30);
        builder.Property(i => i.StockActual).HasColumnName("stock_actual").HasPrecision(12, 3).HasDefaultValue(0m);
        builder.Property(i => i.PrecioReferencia).HasColumnName("precio_referencia").HasPrecision(10, 2).HasDefaultValue(0m);
        builder.Property(i => i.Activo).HasColumnName("activo").HasDefaultValue(true);
        builder.Property(i => i.FechaRegistro).HasColumnName("fecha_registro");
    }
}
