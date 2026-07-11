using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Termales.Entities.Models.Caja;
using Termales.Entities.Models.Compras;

namespace Termales.DAL.Configurations.Compras;

public class CompraConfiguration : IEntityTypeConfiguration<Compra>
{
    public void Configure(EntityTypeBuilder<Compra> builder)
    {
        builder.ToTable("compras", "compras");
        builder.HasKey(c => c.CompraId);
        builder.Property(c => c.CompraId).HasColumnName("compra_id").ValueGeneratedOnAdd();
        builder.Property(c => c.ProveedorId).HasColumnName("proveedor_id");
        builder.Property(c => c.TipoComprobante).HasColumnName("tipo_comprobante").HasMaxLength(20).IsRequired();
        builder.Property(c => c.Serie).HasColumnName("serie").HasMaxLength(10).IsRequired();
        builder.Property(c => c.Numero).HasColumnName("numero").IsRequired();
        builder.Property(c => c.FechaEmision).HasColumnName("fecha_emision");
        builder.Property(c => c.FormaPago).HasColumnName("forma_pago").HasMaxLength(20).IsRequired();
        builder.Property(c => c.FechaVencimiento).HasColumnName("fecha_vencimiento");
        builder.Property(c => c.Moneda).HasColumnName("moneda").HasMaxLength(3).HasDefaultValue("PEN");
        builder.Property(c => c.TotalGravada).HasColumnName("total_gravada").HasPrecision(10, 2);
        builder.Property(c => c.Igv).HasColumnName("igv").HasPrecision(10, 2);
        builder.Property(c => c.Total).HasColumnName("total").HasPrecision(10, 2);
        builder.Property(c => c.Estado).HasColumnName("estado").HasMaxLength(20).IsRequired();
        builder.Property(c => c.Observaciones).HasColumnName("observaciones").HasMaxLength(400);
        builder.Property(c => c.RegistradoPor).HasColumnName("registrado_por").HasMaxLength(150).IsRequired();
        builder.Property(c => c.FechaRegistro).HasColumnName("fecha_registro").HasDefaultValueSql("now()");
        builder.Property(c => c.FechaPago).HasColumnName("fecha_pago");
        builder.Property(c => c.EgresoCajaChicaId).HasColumnName("egreso_caja_chica_id");

        builder.HasOne(c => c.Proveedor)
               .WithMany()
               .HasForeignKey(c => c.ProveedorId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<EgresoCajaChica>()
               .WithMany()
               .HasForeignKey(c => c.EgresoCajaChicaId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(c => new { c.ProveedorId, c.Serie, c.Numero }).IsUnique();
    }
}
