using System.Globalization;
using System.Net.Sockets;
using System.Text;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Termales.BLL.Interfaces.Comedor;
using Termales.Common.Settings;
using Termales.Entities.Models.Comedor;

namespace Termales.BLL.Services.Comedor;

public class ComandaPrinterService : IComandaPrinterService
{
    private readonly ImpresoraComandaSettings _cfg;
    private readonly IHubContext<ComandaHub> _hub;

    private const byte ESC = 0x1B;
    private const byte GS  = 0x1D;

    public ComandaPrinterService(IOptions<ImpresoraComandaSettings> cfg, IHubContext<ComandaHub> hub)
    {
        _cfg = cfg.Value;
        _hub = hub;
    }

    public async Task ImprimirAsync(Orden orden, IEnumerable<OrdenDetalle> detalles, string titulo)
    {
        if (!_cfg.Activa) return;

        try
        {
            var bytes = ConstruirTicket(orden, detalles, titulo);

            if (string.Equals(_cfg.Modo, "bridge", StringComparison.OrdinalIgnoreCase))
                await ImprimirBridgeAsync(bytes);
            else if (string.Equals(_cfg.Modo, "usb", StringComparison.OrdinalIgnoreCase))
            {
                if (!OperatingSystem.IsWindows())
                    throw new PlatformNotSupportedException("El modo de impresión \"usb\" solo está disponible cuando el API corre en Windows");
                await ImprimirUsbAsync(bytes);
            }
            else
                await ImprimirRedAsync(bytes);
        }
        catch (Exception ex)
        {
            // La orden ya se guardó en base de datos; un fallo de impresión
            // (impresora apagada, sin red, sin USB, sin puente conectado, etc.)
            // no debe tumbar la request.
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[ComandaPrinter] No se pudo imprimir la comanda de la orden #{orden.OrdenId}: {ex.Message}");
            Console.ResetColor();
        }
    }

    // La API (ej. corriendo en un VPS en la nube) no tiene acceso directo a la
    // impresora física del negocio; en vez de conectar, transmite el ticket ya
    // armado (Base64) por SignalR a un "puente" local que sí está junto a la
    // impresora y lo imprime (ver proyecto Termales.ImpresoraPuente).
    private Task ImprimirBridgeAsync(byte[] bytes) =>
        _hub.Clients.Group("impresoras").SendAsync("ImprimirComanda", Convert.ToBase64String(bytes));

    private async Task ImprimirRedAsync(byte[] bytes)
    {
        using var cliente = new TcpClient();
        var conexion = cliente.ConnectAsync(_cfg.Ip, _cfg.Puerto);
        if (await Task.WhenAny(conexion, Task.Delay(_cfg.TimeoutMs)) != conexion)
            throw new TimeoutException($"No se pudo conectar a la impresora {_cfg.Ip}:{_cfg.Puerto}");

        using var stream = cliente.GetStream();
        await stream.WriteAsync(bytes);
        await stream.FlushAsync();
    }

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    private Task ImprimirUsbAsync(byte[] bytes)
    {
        // La impresión USB usa el spooler de Windows (winspool.drv), que es
        // síncrono/bloqueante; se corre en un hilo de threadpool para no
        // bloquear el hilo de la request. El API corre en la PC de la
        // impresora (Windows), por lo que esta ruta siempre es válida ahí.
        return Task.Run(() => RawPrinterHelper.SendBytesToPrinter(_cfg.NombreImpresora, bytes));
    }

    private byte[] ConstruirTicket(Orden orden, IEnumerable<OrdenDetalle> detalles, string titulo)
    {
        var ancho = _cfg.AnchoTicket;
        var linea = new string('-', ancho);

        var cuerpo = new StringBuilder();
        cuerpo.AppendLine(linea);
        cuerpo.AppendLine($"Mesa: {orden.Mesa?.Numero.ToString() ?? "-"}");
        cuerpo.AppendLine($"Mesero: {NombreMesero(orden)}");
        cuerpo.AppendLine($"Orden #{orden.OrdenId}  {DateTime.Now:dd/MM HH:mm}");
        cuerpo.AppendLine(linea);

        foreach (var d in detalles)
        {
            cuerpo.AppendLine($"x{d.Cantidad}  {d.ItemMenu?.Nombre ?? $"Item {d.ItemMenuId}"}");
            if (!string.IsNullOrWhiteSpace(d.Observaciones))
                cuerpo.AppendLine($"    obs: {d.Observaciones}");
        }

        cuerpo.AppendLine(linea);
        if (!string.IsNullOrWhiteSpace(orden.Observaciones))
        {
            cuerpo.AppendLine($"Obs. orden: {orden.Observaciones}");
            cuerpo.AppendLine(linea);
        }
        cuerpo.AppendLine();
        cuerpo.AppendLine();
        cuerpo.AppendLine();

        using var ms = new MemoryStream();
        ms.WriteByte(ESC); ms.WriteByte(0x40);                       // ESC @  — reset/inicializar
        ms.WriteByte(ESC); ms.WriteByte(0x61); ms.WriteByte(0x01);   // ESC a 1 — centrar
        ms.WriteByte(ESC); ms.WriteByte(0x45); ms.WriteByte(0x01);   // ESC E 1 — negrita on
        ms.Write(Encoding.ASCII.GetBytes(QuitarTildes(CentrarTexto(titulo, ancho)) + "\n"));
        ms.WriteByte(ESC); ms.WriteByte(0x45); ms.WriteByte(0x00);   // negrita off
        ms.WriteByte(ESC); ms.WriteByte(0x61); ms.WriteByte(0x00);   // alinear izquierda
        ms.Write(Encoding.ASCII.GetBytes(QuitarTildes(cuerpo.ToString())));

        ms.WriteByte(GS); ms.WriteByte(0x56); ms.WriteByte(0x01);    // GS V 1 — corte parcial

        return ms.ToArray();
    }

    private static string NombreMesero(Orden orden) =>
        orden.Usuario?.Empleado is not null ? $"{orden.Usuario.Empleado.Nombres} {orden.Usuario.Empleado.Apellidos}".Trim() : "-";

    private static string CentrarTexto(string texto, int ancho)
    {
        if (texto.Length >= ancho) return texto;
        var espacios = (ancho - texto.Length) / 2;
        return new string(' ', espacios) + texto;
    }

    // Los ESC/POS de red suelen venir configurados en codepages tipo CP437/850
    // que no siempre coinciden con lo que manda el server; en vez de negociar
    // codepages por modelo de impresora, se quitan tildes/eñes para garantizar
    // texto legible en cualquier impresora térmica sin mojibake.
    private static string QuitarTildes(string texto)
    {
        var normalizado = texto.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in normalizado)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
}
