using System.Globalization;
using Termales.Entities.Enums;

namespace Termales.Common.Utils;

/// <summary>
/// Formato de texto compartido entre los distintos generadores de ticket/comprobante
/// (PDF de Boleta/Factura, ticket ESC/POS de caja) para que muestren lo mismo: el
/// monto en letras y la forma de pago con su monto.
/// </summary>
public static class TicketFormato
{
    // Se muestra directo el medio y el monto (ej. "EFECTIVO: S/ 40.00"), sin la
    // etiqueta genérica "Forma de pago: ..." — así se ve de un vistazo cuánto
    // entró por cada medio. En Mixto se desglosa efectivo + Yape/Plin.
    public static string FormaDePagoTexto(MetodoPago metodoPago, decimal total, decimal? montoEfectivoMixto) => metodoPago switch
    {
        MetodoPago.Efectivo => $"EFECTIVO: S/ {total.ToString("F2", CultureInfo.InvariantCulture)}",
        MetodoPago.YapePlin => $"YAPE/PLIN: S/ {total.ToString("F2", CultureInfo.InvariantCulture)}",
        MetodoPago.Transferencia => $"TRANSFERENCIA: S/ {total.ToString("F2", CultureInfo.InvariantCulture)}",
        MetodoPago.Fiado => $"CRÉDITO: S/ {total.ToString("F2", CultureInfo.InvariantCulture)}",
        MetodoPago.Mixto => FormatoMixto(total, montoEfectivoMixto),
        _ => metodoPago.ToString(),
    };

    private static string FormatoMixto(decimal total, decimal? montoEfectivoMixto)
    {
        var efectivo = montoEfectivoMixto ?? 0;
        var otros = Math.Max(0, total - efectivo);
        return $"EFECTIVO: S/ {efectivo.ToString("F2", CultureInfo.InvariantCulture)} + YAPE/PLIN: S/ {otros.ToString("F2", CultureInfo.InvariantCulture)}";
    }

    // ── Monto en letras (ej. "DIEZ CON 00/100 SOLES") ──
    private static readonly string[] Unidades = { "", "UNO", "DOS", "TRES", "CUATRO", "CINCO", "SEIS", "SIETE", "OCHO", "NUEVE" };
    private static readonly string[] Diecis   = { "DIEZ", "ONCE", "DOCE", "TRECE", "CATORCE", "QUINCE", "DIECISÉIS", "DIECISIETE", "DIECIOCHO", "DIECINUEVE" };
    private static readonly string[] Veintis  = { "VEINTE", "VEINTIUNO", "VEINTIDÓS", "VEINTITRÉS", "VEINTICUATRO", "VEINTICINCO", "VEINTISÉIS", "VEINTISIETE", "VEINTIOCHO", "VEINTINUEVE" };
    private static readonly string[] Decenas  = { "", "", "", "TREINTA", "CUARENTA", "CINCUENTA", "SESENTA", "SETENTA", "OCHENTA", "NOVENTA" };
    private static readonly string[] Centenas = { "", "CIENTO", "DOSCIENTOS", "TRESCIENTOS", "CUATROCIENTOS", "QUINIENTOS", "SEISCIENTOS", "SETECIENTOS", "OCHOCIENTOS", "NOVECIENTOS" };

    private static string ConvertirDecenas(int n)
    {
        if (n == 0) return "";
        if (n < 10) return Unidades[n];
        if (n < 20) return Diecis[n - 10];
        if (n < 30) return Veintis[n - 20];
        var d = n / 10;
        var u = n % 10;
        return u == 0 ? Decenas[d] : $"{Decenas[d]} Y {Unidades[u]}";
    }

    private static string ConvertirCentenas(int n)
    {
        if (n == 0) return "";
        if (n == 100) return "CIEN";
        var c = n / 100;
        var resto = n % 100;
        var partes = new List<string>();
        if (c > 0) partes.Add(Centenas[c]);
        if (resto > 0) partes.Add(ConvertirDecenas(resto));
        return string.Join(" ", partes);
    }

    // "veintiuno"/"...uno" pierde la "o" final antes de "mil"/"millón" (veintiún mil, treinta y un mil).
    private static string Apocopar(string texto)
    {
        if (texto.EndsWith("VEINTIUNO")) return texto[..^9] + "VEINTIÚN";
        if (texto.EndsWith("UNO")) return texto[..^3] + "UN";
        return texto;
    }

    private static string ConvertirEntero(int n)
    {
        if (n == 0) return "CERO";
        if (n < 1000) return ConvertirCentenas(n);

        if (n < 1_000_000)
        {
            var miles = n / 1000;
            var resto = n % 1000;
            var milesTexto = miles == 1 ? "MIL" : $"{Apocopar(ConvertirCentenas(miles))} MIL";
            return resto == 0 ? milesTexto : $"{milesTexto} {ConvertirCentenas(resto)}";
        }

        var millones = n / 1_000_000;
        var restoM = n % 1_000_000;
        var millonesTexto = millones == 1 ? "UN MILLÓN" : $"{Apocopar(ConvertirEntero(millones))} MILLONES";
        return restoM == 0 ? millonesTexto : $"{millonesTexto} {ConvertirEntero(restoM)}";
    }

    public static string MontoEnLetras(decimal monto, string moneda = "SOLES")
    {
        var redondeado = Math.Round(monto, 2);
        var entero = (int)Math.Floor(redondeado);
        var centavos = (int)Math.Round((redondeado - entero) * 100);
        return $"{ConvertirEntero(entero)} CON {centavos:D2}/100 {moneda}";
    }
}
