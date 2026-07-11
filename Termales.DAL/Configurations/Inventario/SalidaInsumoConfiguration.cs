using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Termales.Entities.Models.Inventario;

namespace Termales.DAL.Configurations.Inventario;

public class SalidaInsumoConfiguration : IEntityTypeConfiguration<SalidaInsumo>
{
    public void Configure(EntityTypeBuilder<SalidaInsumo> builder)
    {
        builder.ToTable("salidas_insumo", "inventario");
        builder.HasKey(s => s.SalidaInsumoId);
        builder.Property(s => s.SalidaInsumoId).HasColumnName("salida_insumo_id");
        builder.Property(s => s.InsumoId).HasColumnName("insumo_id");
        builder.Property(s => s.Cantidad).HasColumnName("cantidad").HasPrecision(12, 3);
        builder.Property(s => s.Fecha).HasColumnName("fecha").HasDefaultValueSql("now()");
        builder.Property(s => s.Observacion).HasColumnName("observacion").HasMaxLength(300);

        builder.HasOne(s => s.Insumo)
            .WithMany()
            .HasForeignKey(s => s.InsumoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
