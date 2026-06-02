using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Termales.Entities.Models;

namespace Termales.DAL.Configurations;

public class PiscinaConfiguration : IEntityTypeConfiguration<Piscina>
{
    public void Configure(EntityTypeBuilder<Piscina> builder)
    {
        builder.ToTable("piscinas");
        builder.HasKey(p => p.PiscinaId);
        builder.Property(p => p.PiscinaId).HasColumnName("piscina_id").ValueGeneratedOnAdd();
        builder.Property(p => p.Nombre).HasColumnName("nombre").HasMaxLength(100).IsRequired();
        builder.Property(p => p.Descripcion).HasColumnName("descripcion").HasMaxLength(500);
        builder.Property(p => p.TemperaturaGrados).HasColumnName("temperatura_grados").HasColumnType("decimal(5,2)");
        builder.Property(p => p.CapacidadPersonas).HasColumnName("capacidad_personas");
        builder.Property(p => p.TarifaPorHora).HasColumnName("tarifa_por_hora").HasColumnType("decimal(10,2)");
        builder.Property(p => p.Disponible).HasColumnName("disponible").HasDefaultValue(true);
    }
}
