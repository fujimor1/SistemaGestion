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

    /// <summary>
    /// "cocina" | "caja" | "ambas". Determina a qué grupo(s) del hub se une
    /// este puente. Con "ambas", la misma impresora conectada a esta PC
    /// imprime tanto comandas de cocina como boletas de caja (útil mientras
    /// solo hay una impresora física en el negocio).
    /// </summary>
    public string Rol             { get; set; } = "ambas";
}
