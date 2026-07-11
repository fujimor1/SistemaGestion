using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Termales.Entities.Models;

namespace Termales.DAL.Configurations;

public class PaqueteBanioConfiguration : IEntityTypeConfiguration<PaqueteBanio>
{
    public void Configure(EntityTypeBuilder<PaqueteBanio> builder)
    {
        builder.ToTable("paquetes_banio");
        builder.HasKey(p => p.PaqueteBanioId);
        builder.Property(p => p.PaqueteBanioId).HasColumnName("paquete_banio_id").ValueGeneratedOnAdd();
        builder.Property(p => p.Nombre).HasColumnName("nombre").HasMaxLength(100).IsRequired();
        builder.Property(p => p.Precio).HasColumnName("precio").HasPrecision(10, 2);
        builder.Property(p => p.Activo).HasColumnName("activo").HasDefaultValue(true);
    }
}

public class PaqueteBanioTipoServicioConfiguration : IEntityTypeConfiguration<PaqueteBanioTipoServicio>
{
    public void Configure(EntityTypeBuilder<PaqueteBanioTipoServicio> builder)
    {
        builder.ToTable("paquete_banio_tipo_servicios");
        builder.HasKey(pt => new { pt.PaqueteBanioId, pt.TipoServicioId });
        builder.Property(pt => pt.PaqueteBanioId).HasColumnName("paquete_banio_id");
        builder.Property(pt => pt.TipoServicioId).HasColumnName("tipo_servicio_id");

        builder.HasOne(pt => pt.PaqueteBanio)
               .WithMany(p => p.Tipos)
               .HasForeignKey(pt => pt.PaqueteBanioId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pt => pt.TipoServicio)
               .WithMany()
               .HasForeignKey(pt => pt.TipoServicioId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
