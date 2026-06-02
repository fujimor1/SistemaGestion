using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Termales.Entities.Models;

namespace Termales.DAL.Configurations;

public class AforoConfiguration : IEntityTypeConfiguration<Aforo>
{
    public void Configure(EntityTypeBuilder<Aforo> builder)
    {
        builder.ToTable("aforos");
        builder.HasKey(a => a.AforoId);
        builder.Property(a => a.AforoId).HasColumnName("aforo_id").ValueGeneratedOnAdd();
        builder.Property(a => a.TipoServicioId).HasColumnName("tipo_servicio_id");
        builder.Property(a => a.Fecha).HasColumnName("fecha");
        builder.Property(a => a.CapacidadMaxima).HasColumnName("capacidad_maxima");
        builder.Property(a => a.OcupacionActual).HasColumnName("ocupacion_actual");

        builder.Ignore(a => a.LugaresDisponibles);

        builder.HasIndex(a => new { a.TipoServicioId, a.Fecha }).IsUnique();

        builder.HasOne(a => a.TipoServicio)
               .WithMany(ts => ts.Aforos)
               .HasForeignKey(a => a.TipoServicioId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
