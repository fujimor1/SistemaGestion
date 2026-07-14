namespace Termales.Common.Settings;

/// <summary>Datos de la empresa que van en la cabecera de los comprobantes/tickets.</summary>
public class EmpresaSettings
{
    public string RazonSocial { get; set; } = "EMP. COMUNAL BAÑOS TERMOMEDICINALES DE COLLPA";
    public string Ruc         { get; set; } = "20284587970";
    public string Direccion   { get; set; } = "CAR. FUJIMORI FUJIMORI NRO. S/N ---- SANTA CATALINA LIMA/HUARAL/SANTA CRUZ DE ANDAMARCA";

    // Campos requeridos por el Anexo N.° 9-A de SUNAT (domicilio fiscal del emisor en el XML UBL 2.1).
    // Sin valor por defecto confiable — deben completarse en el appsettings.json real antes de emitir en producción.
    public string Urbanizacion  { get; set; } = "-";
    public string Distrito      { get; set; } = "";
    public string Provincia     { get; set; } = "";
    public string Departamento  { get; set; } = "";
    public string Ubigeo        { get; set; } = ""; // código INEI de 6 dígitos, catálogo 13
    public string CodigoPais    { get; set; } = "PE";
    public string CodigoEstablecimiento { get; set; } = "0000"; // 0000 = domicilio fiscal, sin anexo
}
