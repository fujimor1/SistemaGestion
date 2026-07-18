using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Termales.Entities.Models.Caja;
using Termales.Entities.Models.Compras;

namespace Termales.DAL.Configurations.Caja;

public class EgresoCajaChicaConfiguration : IEntityTypeConfiguration<EgresoCajaChica>
{
    public void Configure(EntityTypeBuilder<EgresoCajaChica> builder)
    {
        builder.ToTable("egresos_caja_chica", "caja");
        builder.HasKey(e => e.EgresoCajaChicaId);
        builder.Property(e => e.EgresoCajaChicaId).HasColumnName("egreso_caja_chica_id");
        builder.Property(e => e.Fecha).HasColumnName("fecha").HasDefaultValueSql("now()");
        builder.Property(e => e.Concepto).HasColumnName("concepto").HasMaxLength(200).IsRequired();
        builder.Property(e => e.Monto).HasColumnName("monto").HasPrecision(12, 2);
        builder.Property(e => e.Responsable).HasColumnName("responsable").HasMaxLength(150).IsRequired();
        builder.Property(e => e.TipoDocumento).HasColumnName("tipo_documento").HasMaxLength(30);
        builder.Property(e => e.NumeroDocumento).HasColumnName("numero_documento").HasMaxLength(50);
        builder.Property(e => e.RegistradoPor).HasColumnName("registrado_por").HasMaxLength(150);
        builder.Property(e => e.Observaciones).HasColumnName("observaciones").HasMaxLength(300);
        builder.Property(e => e.CompraId).HasColumnName("compra_id");
        builder.HasIndex(e => e.Fecha);

        builder.HasOne<Compra>()
               .WithMany()
               .HasForeignKey(e => e.CompraId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
