using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Termales.Entities.Models;

namespace Termales.DAL.Configurations;

public class ReservaServicioConfiguration : IEntityTypeConfiguration<ReservaServicio>
{
    public void Configure(EntityTypeBuilder<ReservaServicio> builder)
    {
        builder.ToTable("reserva_servicios");
        builder.HasKey(rs => rs.ReservaServicioId);
        builder.Property(rs => rs.ReservaServicioId).HasColumnName("reserva_servicio_id").ValueGeneratedOnAdd();
        builder.Property(rs => rs.ReservaId).HasColumnName("reserva_id");
        builder.Property(rs => rs.ServicioId).HasColumnName("servicio_id");
        builder.Property(rs => rs.Cantidad).HasColumnName("cantidad");
        builder.Property(rs => rs.PrecioUnitario).HasColumnName("precio_unitario").HasColumnType("decimal(10,2)");

        builder.HasOne(rs => rs.Reserva).WithMany(r => r.ReservaServicios).HasForeignKey(rs => rs.ReservaId);
        builder.HasOne(rs => rs.Servicio).WithMany(s => s.ReservaServicios).HasForeignKey(rs => rs.ServicioId);
    }
}
