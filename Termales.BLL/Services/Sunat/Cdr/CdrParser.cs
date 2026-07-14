using System.IO.Compression;
using System.Xml.Linq;
using Termales.BLL.Interfaces.Sunat;

namespace Termales.BLL.Services.Sunat.Cdr;

public class CdrParser : ICdrParser
{
    public ResultadoCdr Parsear(byte[] cdrZip)
    {
        using var memoria = new MemoryStream(cdrZip);
        using var zip = new ZipArchive(memoria, ZipArchiveMode.Read);
        var entrada = zip.Entries.FirstOrDefault(e => e.Name.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException("El ZIP del CDR no contiene ningún archivo XML.");

        using var stream = entrada.Open();
        var xml = XDocument.Load(stream);

        var responseCode = xml.Descendants().FirstOrDefault(e => e.Name.LocalName == "ResponseCode")?.Value
            ?? throw new InvalidOperationException("El CDR no trae ResponseCode.");
        var description = xml.Descendants().FirstOrDefault(e => e.Name.LocalName == "Description")?.Value ?? "";

        return new ResultadoCdr(int.Parse(responseCode), description);
    }
}
