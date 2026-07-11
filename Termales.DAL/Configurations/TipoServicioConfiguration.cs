using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Termales.Entities.Models;

namespace Termales.DAL.Configurations;

public class TipoServicioConfiguration : IEntityTypeConfiguration<TipoServicio>
{
    public void Configure(EntityTypeBuilder<TipoServicio> builder)
    {
        builder.ToTable("tipo_servicios");
        builder.HasKey(t => t.TipoServicioId);
        builder.Property(t => t.TipoServicioId).HasColumnName("tipo_servicio_id").ValueGeneratedOnAdd();
        builder.Property(t => t.Nombre).HasColumnName("nombre").HasMaxLength(100).IsRequired();
        builder.Property(t => t.Descripcion).HasColumnName("descripcion").HasMaxLength(500);
        builder.Property(t => t.CapacidadMaxima).HasColumnName("capacidad_maxima");
        builder.Property(t => t.PrecioPorPersona).HasColumnName("precio_por_persona").HasColumnType("decimal(10,2)");
        builder.Property(t => t.Activo).HasColumnName("activo").HasDefaultValue(true);

        builder.HasData(
            new TipoServicio { TipoServicioId = 1, Nombre = "Piscina", Descripcion = "Acceso a piscina termal", CapacidadMaxima = 50, PrecioPorPersona = 5.00m, Activo = true },
            new TipoServicio { TipoServicioId = 2, Nombre = "Baño Privado", Descripcion = "Baño privado con agua termal", CapacidadMaxima = 4, PrecioPorPersona = 5.00m, Activo = true }
        );
    }
}
