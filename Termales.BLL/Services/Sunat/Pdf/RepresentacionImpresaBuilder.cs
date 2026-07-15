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
                    col.Item().Text(empresa.RazonSocial).Bold().FontSize(12);
                    col.Item().Text($"RUC: {empresa.Ruc}");
                    col.Item().Text(empresa.Direccion);

                    col.Item().PaddingTop(6).Text(titulo).Bold();
                    col.Item().Text($"{comprobante.Serie}-{comprobante.Numero}").Bold().FontSize(11);
                    col.Item().Text($"Fecha de emisión: {fechaLocal:yyyy-MM-dd}");
                    if (referenciaOrigen is not null)
                        col.Item().Text(referenciaOrigen);

                    col.Item().PaddingTop(8).Text("Cliente").Bold();
                    col.Item().Text(clienteDocLabel);
                    col.Item().Text(clienteNombre);

                    col.Item().PaddingTop(8).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(4);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text("Descripción").Bold();
                            header.Cell().Text("Cant.").Bold();
                            header.Cell().Text("Importe").Bold();
                        });

                        foreach (var detalle in comprobante.Detalles)
                        {
                            table.Cell().Text(detalle.Descripcion);
                            table.Cell().Text(detalle.Cantidad.ToString(CultureInfo.InvariantCulture));
                            table.Cell().Text(detalle.Subtotal.ToString("F2", CultureInfo.InvariantCulture));
                        }
                    });

                    col.Item().PaddingTop(8).AlignRight().Text($"Op. gravada: {comprobante.TotalGravada.ToString("F2", CultureInfo.InvariantCulture)}");
                    col.Item().AlignRight().Text($"IGV: {comprobante.Impuesto.ToString("F2", CultureInfo.InvariantCulture)}");
                    col.Item().AlignRight().Text($"IMPORTE TOTAL: {comprobante.Total.ToString("F2", CultureInfo.InvariantCulture)}").Bold();

                    // QR en la parte inferior de la representación impresa, por requisito de SUNAT.
                    col.Item().PaddingTop(10).Row(row =>
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
