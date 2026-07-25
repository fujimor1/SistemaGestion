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

    // Perú es UTC-5 fijo (sin horario de verano). No se usa DateTime.Now porque depende
    // de que el reloj/timezone del sistema operativo del servidor esté bien configurado.
    private static DateTime AhoraLima() => DateTime.UtcNow.AddHours(-5);

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
        _hub.Clients.Group("impresoras-cocina").SendAsync("ImprimirComanda", Convert.ToBase64String(bytes));

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

    private static string EtiquetaMesa(Orden orden)
    {
        if (orden.Mesa is null) return "PARA LLEVAR";
        var numeros = new[] { orden.Mesa.Numero }.Concat(orden.Mesa.MesasSecundarias.Select(s => s.Numero)).OrderBy(n => n);
        return $"MESA {string.Join("+", numeros)}";
    }

    private byte[] ConstruirTicket(Orden orden, IEnumerable<OrdenDetalle> detalles, string titulo)
    {
        var ancho = _cfg.AnchoTicket;
        var lineaFina   = new string('-', ancho);
        var lineaGruesa = new string('=', ancho);
        var ahora = AhoraLima();

        using var ms = new MemoryStream();
        ms.WriteByte(ESC); ms.WriteByte(0x40); // ESC @ — reset/inicializar

        // ── Encabezado pequeño: a qué impresora va y qué tipo de aviso es ──
        Alinear(ms, centrado: true);
        EscribirNegrita(ms, $"Impresora COCINA - {titulo}");
        Escribir(ms, lineaGruesa);

        // ── Bloque grande y centrado: orden, hora-mesero, mesa, ambiente —
        // todo en el tamaño más grande posible para que se lea a distancia
        // en la cocina, igual que el resto de la comanda. ──
        EscribirGrande(ms, $"Orden #{orden.OrdenId}");
        EscribirGrande(ms, $"{ahora:HH:mm:ss}- {NombreMesero(orden)}");
        EscribirGrande(ms, $"{EtiquetaMesa(orden)}:");
        EscribirGrande(ms, "AMBIENTE: COMEDOR");
        Alinear(ms, centrado: false);
        Escribir(ms, lineaGruesa);

        // ── Detalle: cabecera de columnas + cada plato en doble alto ──
        Escribir(ms, "Cant.".PadRight(7) + "Producto");
        foreach (var d in detalles)
        {
            ms.WriteByte(GS); ms.WriteByte(0x21); ms.WriteByte(0x02);   // GS ! 2 — triple alto (ancho normal, para no cortar nombres largos)
            ms.WriteByte(ESC); ms.WriteByte(0x45); ms.WriteByte(0x01);  // negrita on
            ms.Write(Encoding.ASCII.GetBytes(QuitarTildes($"{d.Cantidad}  {d.ItemMenu?.Nombre ?? $"Item {d.ItemMenuId}"}") + "\n"));
            ms.WriteByte(ESC); ms.WriteByte(0x45); ms.WriteByte(0x00);  // negrita off
            ms.WriteByte(GS); ms.WriteByte(0x21); ms.WriteByte(0x00);   // tamaño normal
            if (!string.IsNullOrWhiteSpace(d.Observaciones))
                Escribir(ms, $"    obs: {d.Observaciones}");
        }
        Escribir(ms, lineaFina);

        if (!string.IsNullOrWhiteSpace(orden.Observaciones))
        {
            Escribir(ms, $"Obs. orden: {orden.Observaciones}");
            Escribir(ms, lineaFina);
        }

        // ── Pie: fecha completa en español ──
        Alinear(ms, centrado: true);
        Escribir(ms, FechaLarga(ahora));
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

    // Triple alto y doble ancho, en negrita y centrado — para las líneas que
    // hay que leer a distancia (orden, hora-mesero, mesa, ambiente). El ancho
    // se queda en x2 (no x3) para que "AMBIENTE: COMEDOR" y una hora con
    // nombre de mesero largo no se corten a la mitad de la línea.
    private static void EscribirGrande(MemoryStream ms, string texto)
    {
        Alinear(ms, centrado: true);
        ms.WriteByte(ESC); ms.WriteByte(0x45); ms.WriteByte(0x01); // negrita on
        ms.WriteByte(GS);  ms.WriteByte(0x21); ms.WriteByte(0x12); // GS ! 0x12 — triple alto, doble ancho
        ms.Write(Encoding.ASCII.GetBytes(QuitarTildes(texto) + "\n"));
        ms.WriteByte(GS);  ms.WriteByte(0x21); ms.WriteByte(0x00); // tamaño normal
        ms.WriteByte(ESC); ms.WriteByte(0x45); ms.WriteByte(0x00); // negrita off
    }

    private static void Alinear(MemoryStream ms, bool centrado)
    {
        ms.WriteByte(ESC); ms.WriteByte(0x61); ms.WriteByte(centrado ? (byte)0x01 : (byte)0x00); // ESC a
    }

    // Ej. "Domingo 19 de Julio del 2026". Nombres fijos en vez de CultureInfo
    // "es-PE": el servidor Linux de producción puede no tener los datos de
    // globalización (ICU) instalados, y pedir esa cultura lanzaría una
    // excepción que el try/catch de ImprimirAsync atraparía en silencio —
    // dejando de imprimir el ticket entero sin ningún aviso visible.
    private static readonly string[] DiasSemana =
        { "Domingo", "Lunes", "Martes", "Miercoles", "Jueves", "Viernes", "Sabado" };
    private static readonly string[] Meses =
        { "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre" };

    private static string FechaLarga(DateTime fecha) =>
        $"{DiasSemana[(int)fecha.DayOfWeek]} {fecha.Day} de {Meses[fecha.Month - 1]} del {fecha.Year}";

    private static string NombreMesero(Orden orden) =>
        orden.Usuario?.Empleado is not null ? $"{orden.Usuario.Empleado.Nombres} {orden.Usuario.Empleado.Apellidos}".Trim() : "-";

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
