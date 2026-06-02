using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Termales.Entities.Models;

namespace Termales.DAL.Configurations;

public class ReservaConfiguration : IEntityTypeConfiguration<Reserva>
{
    public void Configure(EntityTypeBuilder<Reserva> builder)
    {
        builder.ToTable("reservas");
        builder.HasKey(r => r.ReservaId);
        builder.Property(r => r.ReservaId).HasColumnName("reserva_id").ValueGeneratedOnAdd();
        builder.Property(r => r.ClienteId).HasColumnName("cliente_id");
        builder.Property(r => r.PiscinaId).HasColumnName("piscina_id");
        builder.Property(r => r.FechaReserva).HasColumnName("fecha_reserva");
        builder.Property(r => r.FechaIngreso).HasColumnName("fecha_ingreso");
        builder.Property(r => r.FechaSalida).HasColumnName("fecha_salida");
        builder.Property(r => r.NumeroPersonas).HasColumnName("numero_personas");
        builder.Property(r => r.MontoTotal).HasColumnName("monto_total").HasColumnType("decimal(10,2)");
        builder.Property(r => r.Estado).HasColumnName("estado").HasConversion<int>();
        builder.Property(r => r.Observaciones).HasColumnName("observaciones").HasMaxLength(500);
        builder.Property(r => r.FechaCreacion).HasColumnName("fecha_creacion");

        builder.HasOne(r => r.Cliente).WithMany(c => c.Reservas).HasForeignKey(r => r.ClienteId);
        builder.HasOne(r => r.Piscina).WithMany(p => p.Reservas).HasForeignKey(r => r.PiscinaId);
        builder.HasOne(r => r.Pago).WithOne(p => p.Reserva).HasForeignKey<Pago>(p => p.ReservaId);
    }
}
