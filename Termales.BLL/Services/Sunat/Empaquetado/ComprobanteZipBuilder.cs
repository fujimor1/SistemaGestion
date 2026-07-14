using System.IO.Compression;
using System.Text;
using Termales.BLL.Interfaces.Sunat;

namespace Termales.BLL.Services.Sunat.Empaquetado;

public class ComprobanteZipBuilder : IComprobanteZipBuilder
{
    public ComprobanteZip Construir(string ruc, string tipoDoc, string serie, int numero, string xmlFirmado)
    {
        var nombreBase = $"{ruc}-{tipoDoc}-{serie}-{numero}";

        using var memoria = new MemoryStream();
        using (var zip = new ZipArchive(memoria, ZipArchiveMode.Create, leaveOpen: true))
        {
            var entrada = zip.CreateEntry($"{nombreBase}.xml", CompressionLevel.Optimal);
            using var writer = new StreamWriter(entrada.Open(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            writer.Write(xmlFirmado);
        }

        return new ComprobanteZip($"{nombreBase}.zip", memoria.ToArray());
    }
}
