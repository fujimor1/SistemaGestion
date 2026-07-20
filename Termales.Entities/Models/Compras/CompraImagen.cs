namespace Termales.Entities.Models.Compras;

public class CompraImagen
{
    public int CompraImagenId { get; set; }
    public int CompraId { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    // Ruta absoluta en disco del servidor, fuera de /var/www/collpa-api (esa carpeta
    // se borra por completo en cada deploy — mismo criterio que el .pfx de SUNAT).
    public string RutaArchivo { get; set; } = string.Empty;
    public DateTime FechaSubida { get; set; } = DateTime.UtcNow;

    public Compra? Compra { get; set; }
}
