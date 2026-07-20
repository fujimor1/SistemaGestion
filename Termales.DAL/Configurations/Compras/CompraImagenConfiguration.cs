using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Termales.Entities.Models.Compras;

namespace Termales.DAL.Configurations.Compras;

public class CompraImagenConfiguration : IEntityTypeConfiguration<CompraImagen>
{
    public void Configure(EntityTypeBuilder<CompraImagen> builder)
    {
        builder.ToTable("compra_imagenes", "compras");
        builder.HasKey(i => i.CompraImagenId);
        builder.Property(i => i.CompraImagenId).HasColumnName("compra_imagen_id").ValueGeneratedOnAdd();
        builder.Property(i => i.CompraId).HasColumnName("compra_id");
        builder.Property(i => i.NombreArchivo).HasColumnName("nombre_archivo").HasMaxLength(255).IsRequired();
        builder.Property(i => i.RutaArchivo).HasColumnName("ruta_archivo").HasMaxLength(500).IsRequired();
        builder.Property(i => i.FechaSubida).HasColumnName("fecha_subida").HasDefaultValueSql("now()");

        builder.HasOne(i => i.Compra)
               .WithMany(c => c.Imagenes)
               .HasForeignKey(i => i.CompraId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
