namespace Termales.Common.Settings;

/// <summary>Configuración para la integración directa con SUNAT (SEE - Del Contribuyente).</summary>
public class SunatSettings
{
    public bool   Habilitado          { get; set; } = false; // feature flag: Factura directa vs Nubefact
    public string Ruc                 { get; set; } = "";
    public string UsuarioSol          { get; set; } = ""; // usuario SOL secundario, sin el RUC concatenado
    public string PasswordSol         { get; set; } = "";
    public string CertificadoPath     { get; set; } = ""; // ruta absoluta en el servidor, fuera de /var/www/collpa-api
    public string CertificadoPassword { get; set; } = "";
    public string Ambiente            { get; set; } = "Beta"; // "Beta" | "Produccion"
    public string EndpointBeta        { get; set; } = "https://e-beta.sunat.gob.pe/ol-ti-itcpfegem-beta/billService?wsdl";
    public string EndpointProduccion  { get; set; } = "https://e-factura.sunat.gob.pe/ol-ti-itcpfegem/billService?wsdl";
    public string SerieFactura        { get; set; } = "";
    public string SerieBoleta         { get; set; } = "";
}
