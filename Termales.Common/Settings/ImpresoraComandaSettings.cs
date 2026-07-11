namespace Termales.Common.Settings;

public class ImpresoraComandaSettings
{
    public bool   Activa           { get; set; } = false;

    /// <summary>
    /// "red" (TCP/IP directo) | "usb" (impresora instalada en Windows, vía spooler)
    /// | "bridge" (la API no tiene acceso directo a la impresora — ej. corre en
    /// la nube — y transmite el ticket por SignalR a un "puente" local que imprime).
    /// </summary>
    public string Modo             { get; set; } = "red";

    // Modo "red"
    public string Ip               { get; set; } = string.Empty;
    public int    Puerto           { get; set; } = 9100;
    public int    TimeoutMs        { get; set; } = 3000;

    // Modo "usb" (nombre exacto de la impresora tal como aparece en Windows)
    public string NombreImpresora  { get; set; } = string.Empty;

    public int    AnchoTicket      { get; set; } = 42;
}
