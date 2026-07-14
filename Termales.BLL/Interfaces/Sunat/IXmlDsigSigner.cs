using System.Xml.Linq;

namespace Termales.BLL.Interfaces.Sunat;

/// <summary>XmlFirmado: XML final como texto, listo para empaquetar en ZIP y enviar.
/// DigestValueBase64: el valor de ds:DigestValue (hash del documento canonicalizado) — es el mismo
/// "valor resumen" que exige el código QR de la representación impresa, calculado una sola vez aquí.</summary>
public record ResultadoFirma(string XmlFirmado, string DigestValueBase64);

public interface IXmlDsigSigner
{
    /// <summary>
    /// Firma el documento y devuelve el XML final como texto, listo para empaquetar en ZIP y enviar.
    /// Deliberadamente NO devuelve XDocument: reconstruir el árbol vía LINQ-to-XML después de firmar
    /// invalida la firma (la re-serialización cambia cómo se representan las declaraciones de namespace
    /// redundantes que trae ds:Signature, lo que rompe la canonicalización C14N).
    /// </summary>
    ResultadoFirma Firmar(XDocument documento);
}
