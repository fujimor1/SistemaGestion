using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Termales.Entities.Models.Inventario;

namespace Termales.DAL.Configurations.Inventario;

public class EntradaInsumoConfiguration : IEntityTypeConfiguration<EntradaInsumo>
{
    public void Configure(EntityTypeBuilder<EntradaInsumo> builder)
    {
        builder.ToTable("entradas_insumo", "inventario");
        builder.HasKey(e => e.EntradaInsumoId);
        builder.Property(e => e.EntradaInsumoId).HasColumnName("entrada_insumo_id").ValueGeneratedOnAdd();
        builder.Property(e => e.InsumoId).HasColumnName("insumo_id");
        builder.Property(e => e.Cantidad).HasColumnName("cantidad").HasPrecision(12, 3);
        builder.Property(e => e.PrecioUnitario).HasColumnName("precio_unitario").HasPrecision(10, 2);
        builder.Property(e => e.Total).HasColumnName("total").HasPrecision(12, 2);
        builder.Property(e => e.Fecha).HasColumnName("fecha");
        builder.Property(e => e.Observacion).HasColumnName("observacion").HasMaxLength(400);
        builder.Property(e => e.CompraId).HasColumnName("compra_id");

        builder.HasOne(e => e.Insumo)
               .WithMany(i => i.Entradas)
               .HasForeignKey(e => e.InsumoId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Compra)
               .WithMany()
               .HasForeignKey(e => e.CompraId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
