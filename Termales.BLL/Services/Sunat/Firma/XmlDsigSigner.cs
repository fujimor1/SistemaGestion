using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Extensions.Options;
using Termales.BLL.Interfaces.Sunat;
using Termales.Common.Settings;

namespace Termales.BLL.Services.Sunat.Firma;

/// <summary>
/// Firma el XML UBL con XML-DSig usando SHA1 + RSA-SHA1 + C14N plano (no exclusive) — confirmado
/// contra el manual oficial de SUNAT y contra una implementación de referencia real, no la
/// combinación SHA256+C14N-exclusive que se había asumido tentativamente al inicio.
/// </summary>
public class XmlDsigSigner : IXmlDsigSigner
{
    private const string ExtNs = "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2";
    private readonly SunatSettings _cfg;

    public XmlDsigSigner(IOptions<SunatSettings> cfg) => _cfg = cfg.Value;

    public ResultadoFirma Firmar(XDocument documento)
    {
        var xmlDoc = new XmlDocument { PreserveWhitespace = true };
        using (var reader = documento.CreateReader())
            xmlDoc.Load(reader);

        var root = xmlDoc.DocumentElement ?? throw new InvalidOperationException("El XML no tiene elemento raíz.");
        var extensionContent = InsertarUblExtensionsVacio(xmlDoc, root);

        // X509KeyStorageFlags.Exportable: el servidor es Linux, no aplica StoreLocation de Windows.
        using var certificado = new X509Certificate2(_cfg.CertificadoPath, _cfg.CertificadoPassword, X509KeyStorageFlags.Exportable);

        var signedXml = new SignedXml(xmlDoc) { SigningKey = certificado.GetRSAPrivateKey() };
        signedXml.SignedInfo!.CanonicalizationMethod = SignedXml.XmlDsigC14NTransformUrl;
        signedXml.SignedInfo.SignatureMethod = SignedXml.XmlDsigRSASHA1Url;

        var reference = new Reference(string.Empty) { DigestMethod = SignedXml.XmlDsigSHA1Url };
        reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
        signedXml.AddReference(reference);

        var keyInfo = new KeyInfo();
        keyInfo.AddClause(new KeyInfoX509Data(certificado));
        signedXml.KeyInfo = keyInfo;

        signedXml.ComputeSignature();
        var digestValueBase64 = Convert.ToBase64String(reference.DigestValue!);
        extensionContent.AppendChild(signedXml.GetXml());

        // Serializado directo del XmlDocument ya firmado — nunca reconstruir vía XDocument
        // después de este punto (invalidaría la firma, ver comentario en IXmlDsigSigner).
        // Se usa Utf8StringWriter porque StringWriter.Encoding es UTF-16 por definición (un string
        // .NET siempre es UTF-16 en memoria) y XmlWriter usa esa propiedad para la declaración
        // <?xml ... encoding="..."?>, ignorando XmlWriterSettings.Encoding cuando el destino es un
        // TextWriter — sin este fix, SUNAT recibe un XML que declara "utf-16" con bytes UTF-8 reales
        // y no logra parsear ni el propio tag UBLVersionID.
        using var writer = new Utf8StringWriter();
        using (var xmlWriter = XmlWriter.Create(writer, new XmlWriterSettings { OmitXmlDeclaration = false }))
            xmlDoc.Save(xmlWriter);
        return new ResultadoFirma(writer.ToString(), digestValueBase64);
    }

    private static XmlElement InsertarUblExtensionsVacio(XmlDocument xmlDoc, XmlElement root)
    {
        var ublExtensions = xmlDoc.CreateElement("ext", "UBLExtensions", ExtNs);
        var ublExtension = xmlDoc.CreateElement("ext", "UBLExtension", ExtNs);
        var extensionContent = xmlDoc.CreateElement("ext", "ExtensionContent", ExtNs);

        ublExtension.AppendChild(extensionContent);
        ublExtensions.AppendChild(ublExtension);
        root.InsertBefore(ublExtensions, root.FirstChild);

        return extensionContent;
    }

    private sealed class Utf8StringWriter : StringWriter
    {
        public override System.Text.Encoding Encoding => System.Text.Encoding.UTF8;
    }
}
