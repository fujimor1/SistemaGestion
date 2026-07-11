namespace Termales.ImpresoraPuente;

public class Config
{
    public string ApiBaseUrl      { get; set; } = string.Empty;
    public string Email           { get; set; } = string.Empty;
    public string Password        { get; set; } = string.Empty;

    /// <summary>"red" (TCP/IP) | "usb" (impresora instalada en Windows)</summary>
    public string Modo            { get; set; } = "usb";

    public string Ip              { get; set; } = string.Empty;
    public int    Puerto          { get; set; } = 9100;
    public int    TimeoutMs       { get; set; } = 3000;
    public string NombreImpresora { get; set; } = string.Empty;
}
