namespace Termales.Common.Settings;

public class UploadsSettings
{
    // Ruta absoluta en el servidor, fuera de /var/www/collpa-api (esa carpeta se
    // reemplaza por completo en cada deploy) — mismo criterio que Sunat:CertificadoPath.
    // Si queda vacío (dev local), el servicio usa una carpeta temporal junto al build.
    public string ComprasPath { get; set; } = string.Empty;
}
