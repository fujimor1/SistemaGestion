using Termales.BLL.Services.Sunat.Pdf;

namespace Termales.Tests.Sunat;

public class RepresentacionImpresaBuilderTests
{
    [Fact]
    public void Construir_QrContenidoTieneLos10CamposEnElOrdenCorrecto()
    {
        var comprobante = FacturaMuestras.Comprobante();
        var empresa = FacturaMuestras.Empresa();
        const string digest = "abc123==";

        var contenido = new QrContentBuilder().Construir(comprobante, empresa, digest);
        var campos = contenido.Split('|');

        Assert.Equal(10, campos.Length);
        Assert.Equal(empresa.Ruc, campos[0]);
        Assert.Equal("01", campos[1]);
        Assert.Equal(comprobante.Serie, campos[2]);
        Assert.Equal(comprobante.Numero.ToString(), campos[3]);
        Assert.Equal("15.25", campos[4]);
        Assert.Equal("100.00", campos[5]);
        Assert.Equal("6", campos[7]);
        Assert.Equal(comprobante.ClienteRuc, campos[8]);
        Assert.Equal(digest, campos[9]);
    }

    [Fact]
    public void Generar_ProduceUnPdfNoVacio()
    {
        var comprobante = FacturaMuestras.Comprobante();
        var empresa = FacturaMuestras.Empresa();

        var builder = new RepresentacionImpresaBuilder(new QrContentBuilder());
        var pdf = builder.Generar(comprobante, empresa, "abc123==");

        Assert.NotEmpty(pdf);
        // Firma de archivo PDF: los primeros bytes deben ser "%PDF-"
        Assert.Equal("%PDF-", System.Text.Encoding.ASCII.GetString(pdf, 0, 5));

        var outPath = Path.Combine(Path.GetTempPath(), "collpa-diag", "factura-muestra.pdf");
        Directory.CreateDirectory(Path.GetDirectoryName(outPath)!);
        File.WriteAllBytes(outPath, pdf);
    }
}
