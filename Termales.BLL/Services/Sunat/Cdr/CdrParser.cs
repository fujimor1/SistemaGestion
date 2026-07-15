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
        var xmlCrudo = new StreamReader(stream).ReadToEnd();
        var xml = XDocument.Parse(xmlCrudo);

        var responseCode = xml.Descendants().FirstOrDefault(e => e.Name.LocalName == "ResponseCode")?.Value
            ?? throw new InvalidOperationException("El CDR no trae ResponseCode.");
        var description = xml.Descendants().FirstOrDefault(e => e.Name.LocalName == "Description")?.Value ?? "";

        // Las notas de observación son elementos <cbc:Note> sueltos (fuera de cac:Response, donde
        // vive Description) — se juntan todas por si SUNAT manda más de una.
        var notas = xml.Descendants().Where(e => e.Name.LocalName == "Note").Select(e => e.Value.Trim())
            .Where(n => n.Length > 0).ToList();
        var observaciones = notas.Count > 0 ? string.Join(" | ", notas) : null;

        return new ResultadoCdr(int.Parse(responseCode), description, xmlCrudo, observaciones);
    }
}
