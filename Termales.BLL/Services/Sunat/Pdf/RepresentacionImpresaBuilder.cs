using System.Globalization;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Termales.BLL.Interfaces.Sunat;
using Termales.Common.Settings;
using Termales.Entities.Enums;
using Termales.Entities.Models;

namespace Termales.BLL.Services.Sunat.Pdf;

/// <summary>
/// Genera la representación impresa (PDF) de la Factura, requisito legal que antes resolvía
/// Nubefact de forma invisible. Contenido y ubicación del QR según el Anexo técnico de SUNAT
/// (código QR, nivel de corrección Q, parte inferior de la representación impresa).
/// </summary>
public class RepresentacionImpresaBuilder : IRepresentacionImpresaBuilder
{
    private readonly IQrContentBuilder _qrContentBuilder;

    static RepresentacionImpresaBuilder()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public RepresentacionImpresaBuilder(IQrContentBuilder qrContentBuilder) => _qrContentBuilder = qrContentBuilder;

    public byte[] Generar(Comprobante comprobante, EmpresaSettings empresa, string digestValueBase64)
    {
        var titulo = comprobante.TipoComprobante switch
        {
            "FI" => "FACTURA ELECTRÓNICA",
            "NC" => "NOTA DE CRÉDITO ELECTRÓNICA",
            _ => "BOLETA DE VENTA ELECTRÓNICA",
        };
        var qrContenido = _qrContentBuilder.Construir(comprobante, empresa, digestValueBase64);
        var qrPng = GenerarQrPng(qrContenido);
        var fechaLocal = comprobante.FechaEmision.ToUniversalTime().AddHours(-5);
        var clienteNombre = comprobante.ClienteRazonSocial ?? comprobante.ClienteNombre ?? "CLIENTES VARIOS";
        var clienteDocLabel = !string.IsNullOrWhiteSpace(comprobante.ClienteRuc)
            ? $"RUC: {comprobante.ClienteRuc}"
            : $"DNI: {comprobante.ClienteDni ?? "-"}";
        var referenciaOrigen = comprobante.TipoComprobante == "NC" && comprobante.ComprobanteOrigen is not null
            ? $"Afecta a: {comprobante.ComprobanteOrigen.Serie}-{comprobante.ComprobanteOrigen.Numero}"
            : null;

        var documento = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A5);
                page.Margin(20);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Content().Column(col =>
                {
                    col.Spacing(8);

                    // ── Emisor + recuadro con tipo y número de documento ──
                    col.Item().Row(row =>
                    {
                        row.RelativeItem(2).Column(c =>
                        {
                            c.Item().Text(empresa.RazonSocial).Bold().FontSize(12);
                            c.Item().Text($"RUC: {empresa.Ruc}");
                            c.Item().Text(empresa.Direccion);
                        });

                        row.ConstantItem(150).Border(1).BorderColor(Colors.Black).Padding(8).Column(c =>
                        {
                            c.Item().AlignCenter().Text($"RUC {empresa.Ruc}").FontSize(8);
                            c.Item().AlignCenter().Text(titulo).Bold().FontSize(9);
                            c.Item().AlignCenter().Text($"{comprobante.Serie}-{comprobante.Numero}").Bold().FontSize(11);
                        });
                    });

                    // ── Datos del cliente y de la venta, en un solo recuadro ──
                    col.Item().Border(1).BorderColor(Colors.Grey.Darken1).Padding(8).Column(c =>
                    {
                        c.Item().Text(text =>
                        {
                            text.Span("Cliente: ").Bold();
                            text.Span(clienteNombre);
                        });
                        c.Item().Text(clienteDocLabel);
                        c.Item().Text($"Fecha de emisión: {fechaLocal:yyyy-MM-dd}");
                        if (referenciaOrigen is not null)
                            c.Item().Text(referenciaOrigen);
                    });

                    // ── Detalle de ítems, con bordes en cada celda ──
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(4);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Border(1).Background(Colors.Grey.Lighten3).Padding(4).Text("Descripción").Bold();
                            header.Cell().Border(1).Background(Colors.Grey.Lighten3).Padding(4).AlignCenter().Text("Cant.").Bold();
                            header.Cell().Border(1).Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text("Importe").Bold();
                        });

                        foreach (var detalle in comprobante.Detalles)
                        {
                            table.Cell().Border(1).Padding(4).Text(detalle.Descripcion);
                            table.Cell().Border(1).Padding(4).AlignCenter().Text(detalle.Cantidad.ToString(CultureInfo.InvariantCulture));
                            table.Cell().Border(1).Padding(4).AlignRight().Text(detalle.Subtotal.ToString("F2", CultureInfo.InvariantCulture));
                        }
                    });

                    // ── Totales, en un recuadro alineado a la derecha ──
                    col.Item().Row(row =>
                    {
                        row.RelativeItem();
                        row.ConstantItem(180).Border(1).BorderColor(Colors.Black).Padding(8).Column(c =>
                        {
                            c.Item().Row(r =>
                            {
                                r.RelativeItem().Text("Op. gravada:");
                                r.ConstantItem(70).AlignRight().Text(comprobante.TotalGravada.ToString("F2", CultureInfo.InvariantCulture));
                            });
                            c.Item().Row(r =>
                            {
                                r.RelativeItem().Text("IGV:");
                                r.ConstantItem(70).AlignRight().Text(comprobante.Impuesto.ToString("F2", CultureInfo.InvariantCulture));
                            });
                            c.Item().PaddingTop(4).Row(r =>
                            {
                                r.RelativeItem().Text("IMPORTE TOTAL:").Bold();
                                r.ConstantItem(70).AlignRight().Text(comprobante.Total.ToString("F2", CultureInfo.InvariantCulture)).Bold();
                            });
                        });
                    });

                    // ── Monto en letras y forma de pago ──
                    col.Item().PaddingTop(2).Text($"SON: {MontoEnLetras(comprobante.Total)}").FontSize(8).Italic();
                    col.Item().Text(FormaDePagoTexto(comprobante)).FontSize(8);

                    // QR en la parte inferior de la representación impresa, por requisito de SUNAT.
                    col.Item().PaddingTop(4).Row(row =>
                    {
                        row.ConstantItem(120).Image(qrPng);
                        row.RelativeItem().PaddingLeft(10).Column(c =>
                        {
                            c.Item().Text("Representación impresa de la factura electrónica").FontSize(7);
                            c.Item().Text($"Hash: {digestValueBase64}").FontSize(6);
                        });
                    });
                });
            });
        });

        return documento.GeneratePdf();
    }

    // Se muestra directo el medio y el monto (ej. "EFECTIVO S/ 40.00"), sin la
    // etiqueta genérica "Forma de pago: ..." — así se ve de un vistazo cuánto
    // entró por cada medio. En Mixto se desglosa efectivo + Yape/Plin.
    private static string FormaDePagoTexto(Comprobante c) => c.MetodoPago switch
    {
        MetodoPago.Efectivo => $"EFECTIVO S/ {c.Total.ToString("F2", CultureInfo.InvariantCulture)}",
        MetodoPago.YapePlin => $"YAPE / PLIN S/ {c.Total.ToString("F2", CultureInfo.InvariantCulture)}",
        MetodoPago.Transferencia => $"TRANSFERENCIA S/ {c.Total.ToString("F2", CultureInfo.InvariantCulture)}",
        MetodoPago.Fiado => $"CRÉDITO S/ {c.Total.ToString("F2", CultureInfo.InvariantCulture)}",
        MetodoPago.Mixto => FormatoMixto(c),
        _ => c.MetodoPago.ToString(),
    };

    private static string FormatoMixto(Comprobante c)
    {
        var efectivo = c.MontoEfectivoMixto ?? 0;
        var otros = Math.Max(0, c.Total - efectivo);
        return $"EFECTIVO S/ {efectivo.ToString("F2", CultureInfo.InvariantCulture)} + YAPE/PLIN S/ {otros.ToString("F2", CultureInfo.InvariantCulture)}";
    }

    // ── Monto en letras (ej. "DIEZ CON 00/100 SOLES"), como exige SUNAT en el
    // pie de la representación impresa de Boletas y Facturas ──
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

    private static string MontoEnLetras(decimal monto, string moneda = "SOLES")
    {
        var redondeado = Math.Round(monto, 2);
        var entero = (int)Math.Floor(redondeado);
        var centavos = (int)Math.Round((redondeado - entero) * 100);
        return $"{ConvertirEntero(entero)} CON {centavos:D2}/100 {moneda}";
    }

    private static byte[] GenerarQrPng(string contenido)
    {
        using var generador = new QRCodeGenerator();
        using var datosQr = generador.CreateQrCode(contenido, QRCodeGenerator.ECCLevel.Q);
        var pngQr = new PngByteQRCode(datosQr);
        return pngQr.GetGraphic(10);
    }
}
