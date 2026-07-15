using System.Globalization;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Termales.BLL.Interfaces.Sunat;
using Termales.Common.Settings;
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

    private static byte[] GenerarQrPng(string contenido)
    {
        using var generador = new QRCodeGenerator();
        using var datosQr = generador.CreateQrCode(contenido, QRCodeGenerator.ECCLevel.Q);
        var pngQr = new PngByteQRCode(datosQr);
        return pngQr.GetGraphic(10);
    }
}
