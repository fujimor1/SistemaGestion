using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Termales.Entities.Models;

namespace Termales.DAL.Configurations;

public class ComprobanteSerieConfiguration : IEntityTypeConfiguration<ComprobanteSerie>
{
    public void Configure(EntityTypeBuilder<ComprobanteSerie> builder)
    {
        builder.ToTable("comprobante_series");
        builder.HasKey(c => c.Serie);
        builder.Property(c => c.Serie).HasColumnName("serie").HasMaxLength(10);
        builder.Property(c => c.TipoComprobante).HasColumnName("tipo_comprobante").HasMaxLength(5).IsRequired();
        builder.Property(c => c.UltimoNumero).HasColumnName("ultimo_numero").IsRequired();
    }
}
