using System.Globalization;
using System.Net.Sockets;
using System.Text;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Termales.BLL.Interfaces;
using Termales.BLL.Services.Comedor;
using Termales.Common.DTOs.Comprobante;
using Termales.Common.Settings;

namespace Termales.BLL.Services;

public class ReciboPrinterService : IReciboPrinterService
{
    private readonly ImpresoraComandaSettings _cfg;
    private readonly IHubContext<ComandaHub> _hub;

    private const byte ESC = 0x1B;
    private const byte GS  = 0x1D;

    public ReciboPrinterService(IOptions<ImpresoraComandaSettings> cfg, IHubContext<ComandaHub> hub)
    {
        _cfg = cfg.Value;
        _hub = hub;
    }

    public async Task ImprimirAsync(ComprobanteResultadoDto resultado, IEnumerable<ItemReciboDto> items, string clienteLabel)
    {
        if (!_cfg.Activa) return;

        try
        {
            var bytes = ConstruirTicket(resultado, items, clienteLabel);

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
            // El comprobante ya se emitió; un fallo de impresión (impresora
            // apagada, sin red, sin USB, sin puente conectado, etc.) no debe
            // tumbar el cobro.
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[ReciboPrinter] No se pudo imprimir el recibo {resultado.NumeroFormateado}: {ex.Message}");
            Console.ResetColor();
        }
    }

    public async Task ImprimirTicketControlAsync(string titulo, string detalle)
    {
        if (!_cfg.Activa) return;

        try
        {
            var bytes = ConstruirTicketControl(titulo, detalle);

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
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[ReciboPrinter] No se pudo imprimir el ticket de control \"{titulo}\": {ex.Message}");
            Console.ResetColor();
        }
    }

    // Igual que ComandaPrinterService: la API (en la nube) no tiene acceso
    // directo a la impresora física de caja; transmite el ticket por
    // SignalR al puente local, que la tiene conectada (USB/red) y además
    // abre el cajón de dinero al recibirlo.
    private Task ImprimirBridgeAsync(byte[] bytes) =>
        _hub.Clients.Group("impresoras-caja").SendAsync("ImprimirBoleta", Convert.ToBase64String(bytes));

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
    private Task ImprimirUsbAsync(byte[] bytes) =>
        Task.Run(() => RawPrinterHelper.SendBytesToPrinter(_cfg.NombreImpresora, bytes));

    private byte[] ConstruirTicket(ComprobanteResultadoDto resultado, IEnumerable<ItemReciboDto> items, string clienteLabel)
    {
        var ancho = _cfg.AnchoTicket;
        var linea = new string('-', ancho);
        var tipoLabel = resultado.TipoComprobante switch
        {
            "BI" => "BOLETA DE VENTA",
            "FI" => "FACTURA",
            _    => "NOTA DE VENTA",
        };

        var cuerpo = new StringBuilder();
        cuerpo.AppendLine(linea);
        cuerpo.AppendLine($"{tipoLabel} {resultado.NumeroFormateado}");
        cuerpo.AppendLine($"Cliente: {clienteLabel}");
        cuerpo.AppendLine($"Cajero: {resultado.Cajero ?? "-"}");
        cuerpo.AppendLine($"{DateTime.Now:dd/MM/yyyy HH:mm}");
        cuerpo.AppendLine(linea);

        foreach (var i in items)
            cuerpo.AppendLine($"x{i.Cantidad}  {i.Descripcion}  S/ {i.Total:F2}");

        cuerpo.AppendLine(linea);
        cuerpo.AppendLine($"Subtotal: S/ {resultado.TotalGravada:F2}");
        cuerpo.AppendLine($"IGV: S/ {resultado.Impuesto:F2}");
        cuerpo.AppendLine($"TOTAL: S/ {resultado.Total:F2}");
        cuerpo.AppendLine(linea);
        cuerpo.AppendLine();
        cuerpo.AppendLine("Gracias por su visita");
        cuerpo.AppendLine();
        cuerpo.AppendLine();

        using var ms = new MemoryStream();

        // ESC p 0 25 250 — abre el cajón de dinero conectado a la impresora
        // (pin 2 del puerto RJ11/RJ45 de la impresora). Se manda primero
        // para que el cajón se abra de inmediato, sin esperar el corte.
        ms.WriteByte(ESC); ms.WriteByte(0x70); ms.WriteByte(0x00); ms.WriteByte(0x19); ms.WriteByte(0xFA);

        ms.WriteByte(ESC); ms.WriteByte(0x40);                       // ESC @  — reset/inicializar
        ms.WriteByte(ESC); ms.WriteByte(0x61); ms.WriteByte(0x01);   // ESC a 1 — centrar
        ms.WriteByte(ESC); ms.WriteByte(0x45); ms.WriteByte(0x01);   // ESC E 1 — negrita on
        ms.Write(Encoding.ASCII.GetBytes(QuitarTildes(CentrarTexto("BAÑOS TERMALES DE COLLPA", ancho)) + "\n"));
        ms.WriteByte(ESC); ms.WriteByte(0x45); ms.WriteByte(0x00);   // negrita off
        ms.WriteByte(ESC); ms.WriteByte(0x61); ms.WriteByte(0x00);   // alinear izquierda
        ms.Write(Encoding.ASCII.GetBytes(QuitarTildes(cuerpo.ToString())));

        ms.WriteByte(GS); ms.WriteByte(0x56); ms.WriteByte(0x01);    // GS V 1 — corte parcial

        return ms.ToArray();
    }

    // Ticket corto de referencia, sin abrir el cajón (ya se abrió con la
    // boleta principal) — solo para que el cliente lo muestre al ingresar
    // a cada área cubierta por la venta.
    private byte[] ConstruirTicketControl(string titulo, string detalle)
    {
        var ancho = _cfg.AnchoTicket;
        var linea = new string('-', ancho);

        var cuerpo = new StringBuilder();
        cuerpo.AppendLine(linea);
        cuerpo.AppendLine(detalle);
        cuerpo.AppendLine($"{DateTime.Now:dd/MM/yyyy HH:mm}");
        cuerpo.AppendLine(linea);
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

    private static string CentrarTexto(string texto, int ancho)
    {
        if (texto.Length >= ancho) return texto;
        var espacios = (ancho - texto.Length) / 2;
        return new string(' ', espacios) + texto;
    }

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
