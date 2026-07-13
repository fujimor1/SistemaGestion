using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Termales.Entities.Models.Comedor;

namespace Termales.DAL.Configurations.Comedor;

public class OrdenConfiguration : IEntityTypeConfiguration<Orden>
{
    public void Configure(EntityTypeBuilder<Orden> builder)
    {
        builder.ToTable("ordenes", "comedor");
        builder.HasKey(o => o.OrdenId);
        builder.Property(o => o.OrdenId).HasColumnName("orden_id").ValueGeneratedOnAdd();
        builder.Property(o => o.MesaId).HasColumnName("mesa_id");
        builder.Property(o => o.UsuarioId).HasColumnName("usuario_id");
        builder.Property(o => o.Estado).HasColumnName("estado").HasConversion<int>();
        builder.Property(o => o.TipoEntrega).HasColumnName("tipo_entrega").HasMaxLength(20).HasDefaultValue("comedor");
        builder.Property(o => o.Total).HasColumnName("total").HasPrecision(10, 2);
        builder.Property(o => o.Observaciones).HasColumnName("observaciones").HasMaxLength(300);
        builder.Property(o => o.MotivoCancelacion).HasColumnName("motivo_cancelacion").HasMaxLength(300);
        builder.Property(o => o.FechaApertura).HasColumnName("fecha_apertura");
        builder.Property(o => o.FechaCierre).HasColumnName("fecha_cierre");

        builder.HasOne(o => o.Mesa)
               .WithMany(m => m.Ordenes)
               .HasForeignKey(o => o.MesaId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Usuario)
               .WithMany()
               .HasForeignKey(o => o.UsuarioId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
