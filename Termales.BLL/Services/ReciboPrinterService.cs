using System.Globalization;
using System.Net.Sockets;
using System.Text;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Termales.BLL.Interfaces;
using Termales.BLL.Services.Comedor;
using Termales.Common.DTOs.Comprobante;
using Termales.Common.Settings;
using Termales.Common.Utils;
using Termales.Entities.Enums;

namespace Termales.BLL.Services;

public class ReciboPrinterService : IReciboPrinterService
{
    private readonly ImpresoraComandaSettings _cfg;
    private readonly EmpresaSettings _empresa;
    private readonly IHubContext<ComandaHub> _hub;

    private const byte ESC = 0x1B;
    private const byte GS  = 0x1D;

    // Perú es UTC-5 fijo (sin horario de verano). No se usa DateTime.Now porque depende
    // de que el reloj/timezone del sistema operativo del servidor esté bien configurado.
    private static DateTime AhoraLima() => DateTime.UtcNow.AddHours(-5);

    public ReciboPrinterService(IOptions<ImpresoraComandaSettings> cfg, IOptions<EmpresaSettings> empresa, IHubContext<ComandaHub> hub)
    {
        _cfg = cfg.Value;
        _empresa = empresa.Value;
        _hub = hub;
    }

    public async Task ImprimirAsync(
        ComprobanteResultadoDto resultado, IEnumerable<ItemReciboDto> items, string clienteLabel,
        MetodoPago metodoPago, decimal? montoEfectivoMixto)
    {
        if (!_cfg.Activa) return;

        try
        {
            var bytes = ConstruirTicket(resultado, items, clienteLabel, metodoPago, montoEfectivoMixto);

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

    private byte[] ConstruirTicket(
        ComprobanteResultadoDto resultado, IEnumerable<ItemReciboDto> items, string clienteLabel,
        MetodoPago metodoPago, decimal? montoEfectivoMixto)
    {
        var ancho = _cfg.AnchoTicket;
        var lineaFina   = new string('-', ancho);
        var lineaGruesa = new string('=', ancho);
        var tipoLabel = resultado.TipoComprobante switch
        {
            "BI" => "BOLETA DE VENTA",
            "FI" => "FACTURA",
            _    => "NOTA DE VENTA",
        };

        using var ms = new MemoryStream();

        // ESC p 0 25 250 — abre el cajón de dinero conectado a la impresora
        // (pin 2 del puerto RJ11/RJ45 de la impresora). Se manda primero
        // para que el cajón se abra de inmediato, sin esperar el corte.
        ms.WriteByte(ESC); ms.WriteByte(0x70); ms.WriteByte(0x00); ms.WriteByte(0x19); ms.WriteByte(0xFA);
        ms.WriteByte(ESC); ms.WriteByte(0x40); // ESC @ — reset/inicializar

        // ── Encabezado del negocio ──
        Alinear(ms, centrado: true);
        EscribirNegrita(ms, _empresa.RazonSocial);
        if (!string.IsNullOrWhiteSpace(_empresa.Ruc))
            Escribir(ms, $"RUC {_empresa.Ruc}");
        if (!string.IsNullOrWhiteSpace(_empresa.Direccion))
            Escribir(ms, _empresa.Direccion);
        if (!string.IsNullOrWhiteSpace(_empresa.Telefono))
            Escribir(ms, $"Telf: {_empresa.Telefono}");
        Escribir(ms, "");

        // ── Tipo y número de comprobante, destacado entre líneas dobles ──
        Alinear(ms, centrado: false);
        Escribir(ms, lineaGruesa);
        EscribirNegrita(ms, CentrarTexto(tipoLabel, ancho));
        Escribir(ms, CentrarTexto(resultado.NumeroFormateado, ancho));
        Escribir(ms, lineaGruesa);

        // ── Datos de la venta (etiquetas alineadas) ──
        Escribir(ms, $"{"Cliente".PadRight(8)}: {clienteLabel}");
        Escribir(ms, $"{"Fecha".PadRight(8)}: {AhoraLima():dd/MM/yyyy HH:mm}");
        Escribir(ms, lineaFina);

        // ── Detalle de ítems ──
        Escribir(ms, FormatearCabeceraItems(ancho));
        Escribir(ms, lineaFina);
        foreach (var i in items)
            Escribir(ms, FormatearLineaItem(i.Cantidad, i.Descripcion, i.PrecioUnitario, i.Total, ancho));
        Escribir(ms, lineaGruesa);

        // ── Totales (el total final destacado en negrita) ──
        // La Nota de Venta no es un documento tributario — no se desglosa
        // IGV, solo el total. Boleta y Factura sí lo requieren.
        if (resultado.TipoComprobante != "NV")
        {
            Escribir(ms, FormatearLineaMonto("Subtotal s/IGV", resultado.TotalGravada, ancho));
            Escribir(ms, FormatearLineaMonto("IGV (18%)", resultado.Impuesto, ancho));
        }
        EscribirNegrita(ms, FormatearLineaMonto("TOTAL A PAGAR", resultado.Total, ancho));
        Escribir(ms, lineaGruesa);

        // ── Monto en letras y forma de pago ──
        Escribir(ms, $"SON: {TicketFormato.MontoEnLetras(resultado.Total)}");
        Escribir(ms, TicketFormato.FormaDePagoTexto(metodoPago, resultado.Total, montoEfectivoMixto));
        Escribir(ms, $"{"Vendedor".PadRight(8)}: {resultado.Cajero ?? "-"}");
        Escribir(ms, lineaFina);

        // ── Pie ──
        Escribir(ms, "");
        Alinear(ms, centrado: true);
        Escribir(ms, "¡Gracias por su visita!");
        Escribir(ms, "Le esperamos pronto");
        Escribir(ms, "");
        Escribir(ms, "");

        ms.WriteByte(GS); ms.WriteByte(0x56); ms.WriteByte(0x01); // GS V 1 — corte parcial

        return ms.ToArray();
    }

    private static void Escribir(MemoryStream ms, string texto) =>
        ms.Write(Encoding.ASCII.GetBytes(QuitarTildes(texto) + "\n"));

    private static void EscribirNegrita(MemoryStream ms, string texto)
    {
        ms.WriteByte(ESC); ms.WriteByte(0x45); ms.WriteByte(0x01); // ESC E 1 — negrita on
        Escribir(ms, texto);
        ms.WriteByte(ESC); ms.WriteByte(0x45); ms.WriteByte(0x00); // negrita off
    }

    private static void Alinear(MemoryStream ms, bool centrado)
    {
        ms.WriteByte(ESC); ms.WriteByte(0x61); ms.WriteByte(centrado ? (byte)0x01 : (byte)0x00); // ESC a
    }

    // Columnas: cantidad + descripción (resto) + precio unitario + total,
    // ambos alineados a la derecha (ancho fijo, ~9-10 caracteres para "S/ 9999.99").
    // ColCantidad va 1 más que "CAN" (3 letras) para que quede un espacio visible
    // antes de "DESCRIPCION" — antes no lo tenía y salían pegados ("CANTDESCRIPCION").
    private const int ColCantidad = 5;
    private const int ColPrecioUnitario = 9;
    private const int ColTotal    = 10;

    private static string FormatearCabeceraItems(int ancho)
    {
        var anchoDesc = Math.Max(ancho - ColCantidad - ColPrecioUnitario - ColTotal, 4);
        return "CAN".PadRight(ColCantidad) + "DESCRIPCION".PadRight(anchoDesc) + "P.U.".PadLeft(ColPrecioUnitario) + "TOTAL".PadLeft(ColTotal);
    }

    private static string FormatearLineaItem(decimal cantidad, string descripcion, decimal precioUnitario, decimal total, int ancho)
    {
        var anchoDesc = Math.Max(ancho - ColCantidad - ColPrecioUnitario - ColTotal, 4);
        var desc = descripcion.Length > anchoDesc ? descripcion[..(anchoDesc - 1)] + "." : descripcion;
        var puTxt = $"S/{precioUnitario.ToString("F2", CultureInfo.InvariantCulture)}";
        var totalTxt = $"S/{total.ToString("F2", CultureInfo.InvariantCulture)}";
        var cantTxt = cantidad == Math.Truncate(cantidad) ? cantidad.ToString("0", CultureInfo.InvariantCulture) : cantidad.ToString("0.##", CultureInfo.InvariantCulture);
        return cantTxt.PadRight(ColCantidad) + desc.PadRight(anchoDesc) + puTxt.PadLeft(ColPrecioUnitario) + totalTxt.PadLeft(ColTotal);
    }

    private static string FormatearLineaMonto(string label, decimal valor, int ancho)
    {
        var totalTxt = $"S/ {valor.ToString("F2", CultureInfo.InvariantCulture)}";
        return label.PadRight(ancho - totalTxt.Length) + totalTxt;
    }

    // Ticket corto de referencia, sin abrir el cajón (ya se abrió con la
    // boleta principal) — solo para que el cliente lo muestre al ingresar
    // a cada área cubierta por la venta.
    private byte[] ConstruirTicketControl(string titulo, string detalle)
    {
        var ancho = _cfg.AnchoTicket;
        var lineaGruesa = new string('=', ancho);

        using var ms = new MemoryStream();
        ms.WriteByte(ESC); ms.WriteByte(0x40); // ESC @ — reset/inicializar

        Alinear(ms, centrado: true);
        Escribir(ms, lineaGruesa);
        EscribirNegrita(ms, CentrarTexto(titulo, ancho));
        Escribir(ms, lineaGruesa);
        Escribir(ms, "");

        Alinear(ms, centrado: false);
        Escribir(ms, detalle);
        Escribir(ms, $"{AhoraLima():dd/MM/yyyy HH:mm}");
        Escribir(ms, "");
        Escribir(ms, "");

        ms.WriteByte(GS); ms.WriteByte(0x56); ms.WriteByte(0x01); // GS V 1 — corte parcial

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
