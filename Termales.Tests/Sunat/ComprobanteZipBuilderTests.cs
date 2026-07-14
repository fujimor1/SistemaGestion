using System.IO.Compression;
using Termales.BLL.Services.Sunat.Empaquetado;

namespace Termales.Tests.Sunat;

public class ComprobanteZipBuilderTests
{
    [Fact]
    public void Construir_GeneraZipConNombreYContenidoCorrectos()
    {
        var resultado = new ComprobanteZipBuilder().Construir("20284587970", "01", "F001", 1, "<xml>contenido</xml>");

        Assert.Equal("20284587970-01-F001-1.zip", resultado.NombreArchivo);

        using var memoria = new MemoryStream(resultado.ContenidoZip);
        using var zip = new ZipArchive(memoria, ZipArchiveMode.Read);
        var entrada = Assert.Single(zip.Entries);
        Assert.Equal("20284587970-01-F001-1.xml", entrada.Name);

        using var reader = new StreamReader(entrada.Open());
        Assert.Equal("<xml>contenido</xml>", reader.ReadToEnd());
    }
}
