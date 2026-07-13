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
    private readonly EmpresaSettings _empresa;
    private readonly IHubContext<ComandaHub> _hub;

    private const byte ESC = 0x1B;
    private const byte GS  = 0x1D;

    public ReciboPrinterService(IOptions<ImpresoraComandaSettings> cfg, IOptions<EmpresaSettings> empresa, IHubContext<ComandaHub> hub)
    {
        _cfg = cfg.Value;
        _empresa = empresa.Value;
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
        cuerpo.AppendLine(CentrarTexto(tipoLabel, ancho));
        cuerpo.AppendLine(CentrarTexto(resultado.NumeroFormateado, ancho));
        cuerpo.AppendLine(linea);
        cuerpo.AppendLine($"Fecha : {DateTime.Now:dd/MM/yyyy HH:mm}");
        cuerpo.AppendLine($"Cliente: {clienteLabel}");
        cuerpo.AppendLine($"Cajero : {resultado.Cajero ?? "-"}");
        cuerpo.AppendLine(linea);
        cuerpo.AppendLine(FormatearCabeceraItems(ancho));
        cuerpo.AppendLine(new string('-', ancho));

        foreach (var i in items)
            cuerpo.AppendLine(FormatearLineaItem(i.Cantidad, i.Descripcion, i.Total, ancho));

        cuerpo.AppendLine(linea);
        // La Nota de Venta no es un documento tributario — no se desglosa
        // IGV, solo el total. Boleta y Factura sí lo requieren.
        if (resultado.TipoComprobante != "NV")
        {
            cuerpo.AppendLine(FormatearLineaMonto("Subtotal s/IGV", resultado.TotalGravada, ancho));
            cuerpo.AppendLine(FormatearLineaMonto("IGV (18%)", resultado.Impuesto, ancho));
        }
        cuerpo.AppendLine(FormatearLineaMonto("TOTAL", resultado.Total, ancho));
        cuerpo.AppendLine(linea);
        cuerpo.AppendLine();
        cuerpo.AppendLine(CentrarTexto("¡Gracias por su visita!", ancho));
        cuerpo.AppendLine(CentrarTexto("Le esperamos pronto", ancho));
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
        ms.Write(Encoding.ASCII.GetBytes(QuitarTildes(CentrarTexto(_empresa.RazonSocial, ancho)) + "\n"));
        ms.WriteByte(ESC); ms.WriteByte(0x45); ms.WriteByte(0x00);   // negrita off
        if (!string.IsNullOrWhiteSpace(_empresa.Ruc))
            ms.Write(Encoding.ASCII.GetBytes(QuitarTildes(CentrarTexto($"RUC {_empresa.Ruc}", ancho)) + "\n"));
        if (!string.IsNullOrWhiteSpace(_empresa.Direccion))
            ms.Write(Encoding.ASCII.GetBytes(QuitarTildes(CentrarTexto(_empresa.Direccion, ancho)) + "\n"));
        ms.WriteByte(ESC); ms.WriteByte(0x61); ms.WriteByte(0x00);   // alinear izquierda
        ms.Write(Encoding.ASCII.GetBytes(QuitarTildes(cuerpo.ToString())));

        ms.WriteByte(GS); ms.WriteByte(0x56); ms.WriteByte(0x01);    // GS V 1 — corte parcial

        return ms.ToArray();
    }

    // Columnas: cantidad (4) + descripción (resto) + total alineado a la derecha
    // (ancho fijo, ~11 caracteres para "S/ 9999.99").
    private const int ColCantidad = 4;
    private const int ColTotal    = 11;

    private static string FormatearCabeceraItems(int ancho)
    {
        var anchoDesc = Math.Max(ancho - ColCantidad - ColTotal, 4);
        return "CANT".PadRight(ColCantidad) + "DESCRIPCION".PadRight(anchoDesc) + "TOTAL".PadLeft(ColTotal);
    }

    private static string FormatearLineaItem(decimal cantidad, string descripcion, decimal total, int ancho)
    {
        var anchoDesc = Math.Max(ancho - ColCantidad - ColTotal, 4);
        var desc = descripcion.Length > anchoDesc ? descripcion[..(anchoDesc - 1)] + "." : descripcion;
        var totalTxt = $"S/{total:F2}";
        var cantTxt = cantidad == Math.Truncate(cantidad) ? cantidad.ToString("0") : cantidad.ToString("0.##");
        return cantTxt.PadRight(ColCantidad) + desc.PadRight(anchoDesc) + totalTxt.PadLeft(ColTotal);
    }

    private static string FormatearLineaMonto(string label, decimal valor, int ancho)
    {
        var totalTxt = $"S/ {valor:F2}";
        return label.PadRight(ancho - totalTxt.Length) + totalTxt;
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
