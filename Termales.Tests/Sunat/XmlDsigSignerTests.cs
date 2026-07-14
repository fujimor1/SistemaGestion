using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using Microsoft.Extensions.Options;
using Termales.BLL.Services.Sunat.Firma;
using Termales.BLL.Services.Sunat.Xml;
using Termales.Common.Settings;

namespace Termales.Tests.Sunat;

public class XmlDsigSignerTests : IDisposable
{
    private const string PfxPassword = "Test1234!";
    private readonly string _pfxPath = Path.Combine(Path.GetTempPath(), $"collpa-test-cert-{Guid.NewGuid():N}.pfx");

    public XmlDsigSignerTests()
    {
        // Certificado autofirmado, solo para probar la firma localmente — no es el .pfx real de Collpa.
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest("CN=Collpa Test", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        using var cert = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(1));
        File.WriteAllBytes(_pfxPath, cert.Export(X509ContentType.Pfx, PfxPassword));
    }

    public void Dispose() => File.Delete(_pfxPath);

    [Fact]
    public void Firmar_DevuelveXmlConFirmaValidaDentroDeUblExtensions()
    {
        var xmlSinFirmar = new FacturaXmlBuilder().Construir(FacturaMuestras.Comprobante(), FacturaMuestras.Empresa());
        var settings = Options.Create(new SunatSettings { CertificadoPath = _pfxPath, CertificadoPassword = PfxPassword });

        var resultado = new XmlDsigSigner(settings).Firmar(xmlSinFirmar);
        Assert.False(string.IsNullOrWhiteSpace(resultado.DigestValueBase64));

        // Parseo único, directo del string devuelto — así es como se consume en la práctica
        // (empaquetado en ZIP, enviado a SUNAT), sin pasar por XDocument de nuevo.
        var xmlDoc = new XmlDocument { PreserveWhitespace = true };
        xmlDoc.LoadXml(resultado.XmlFirmado);

        var ns = new XmlNamespaceManager(xmlDoc.NameTable);
        ns.AddNamespace("ext", "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2");
        ns.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");

        var signatureNode = xmlDoc.SelectSingleNode(
            "//ext:UBLExtensions/ext:UBLExtension/ext:ExtensionContent/ds:Signature", ns);
        Assert.NotNull(signatureNode);

        var signedXml = new SignedXml(xmlDoc);
        signedXml.LoadXml((XmlElement)signatureNode!);
        Assert.True(signedXml.CheckSignature());
    }
}
