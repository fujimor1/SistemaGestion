namespace Termales.Common.Settings;

public class NubefactSettings
{
    public string Token         { get; set; } = string.Empty;
    public string Ruc           { get; set; } = string.Empty;
    public string SerieNV        { get; set; } = "NV02";
    public string SerieBoleta   { get; set; } = "B001";
    public string SerieFactura  { get; set; } = "F001";
    public string SerieNcBoleta { get; set; } = "BC01";
    public string SerieNcFactura { get; set; } = "FC01";
    public string UrlBase       { get; set; } = "https://api.nubefact.com/api/v1/";
    public bool   ModoSimulacion { get; set; } = true;
}
