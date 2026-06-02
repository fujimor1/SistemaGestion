using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Termales.Entities.Models;

namespace Termales.DAL.Configurations;

public class TurnoConfiguration : IEntityTypeConfiguration<Turno>
{
    public void Configure(EntityTypeBuilder<Turno> builder)
    {
        builder.ToTable("turnos");
        builder.HasKey(t => t.TurnoId);
        builder.Property(t => t.TurnoId).HasColumnName("turno_id").ValueGeneratedOnAdd();
        builder.Property(t => t.TipoServicioId).HasColumnName("tipo_servicio_id");
        builder.Property(t => t.FechaHora).HasColumnName("fecha_hora");
        builder.Property(t => t.CantidadPersonas).HasColumnName("cantidad_personas");
        builder.Property(t => t.MontoTotal).HasColumnName("monto_total").HasColumnType("decimal(10,2)");
        builder.Property(t => t.EstadoPago).HasColumnName("estado_pago").HasConversion<int>();
        builder.Property(t => t.MetodoPago).HasColumnName("metodo_pago").HasConversion<int>();
        builder.Property(t => t.UsuarioId).HasColumnName("usuario_id");
        builder.Property(t => t.FechaCreacion).HasColumnName("fecha_creacion");

        builder.HasOne(t => t.TipoServicio)
               .WithMany(ts => ts.Turnos)
               .HasForeignKey(t => t.TipoServicioId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Usuario)
               .WithMany()
               .HasForeignKey(t => t.UsuarioId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
