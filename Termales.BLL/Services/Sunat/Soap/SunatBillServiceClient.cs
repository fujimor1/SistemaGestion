using System.Text;
using System.Xml.Linq;
using Microsoft.Extensions.Options;
using Termales.BLL.Interfaces.Sunat;
using Termales.Common.Settings;

namespace Termales.BLL.Services.Sunat.Soap;

/// <summary>
/// Cliente SOAP construido a mano contra el webservice billService de SUNAT (sendBill),
/// verificado directamente contra el WSDL/XSD real publicado por SUNAT (namespace, nombres de
/// elementos y SOAPAction "urn:sendBill"), no contra una implementación de terceros.
/// </summary>
public class SunatBillServiceClient : ISunatBillServiceClient
{
    private static readonly XNamespace SoapEnv = "http://schemas.xmlsoap.org/soap/envelope/";
    private static readonly XNamespace Wsse = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd";
    private static readonly XNamespace Svc = "http://service.sunat.gob.pe";

    private readonly HttpClient _http;
    private readonly SunatSettings _cfg;

    public SunatBillServiceClient(HttpClient http, IOptions<SunatSettings> cfg)
    {
        _http = http;
        _cfg = cfg.Value;
    }

    public async Task<ResultadoEnvioSunat> EnviarAsync(string nombreArchivoZip, byte[] contenidoZip, CancellationToken ct = default)
    {
        var endpoint = (_cfg.Ambiente == "Produccion" ? _cfg.EndpointProduccion : _cfg.EndpointBeta)
            .Replace("?wsdl", string.Empty);

        var sobre = new XDocument(
            new XElement(SoapEnv + "Envelope",
                new XElement(SoapEnv + "Header",
                    new XElement(Wsse + "Security",
                        new XElement(Wsse + "UsernameToken",
                            new XElement(Wsse + "Username", $"{_cfg.Ruc}{_cfg.UsuarioSol}"),
                            new XElement(Wsse + "Password", _cfg.PasswordSol)))),
                new XElement(SoapEnv + "Body",
                    new XElement(Svc + "sendBill",
                        new XElement("fileName", nombreArchivoZip),
                        new XElement("contentFile", Convert.ToBase64String(contenidoZip))))));

        using var contenido = new StringContent(sobre.ToString(SaveOptions.DisableFormatting), Encoding.UTF8, "text/xml");
        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint) { Content = contenido };
        request.Headers.Add("SOAPAction", "\"urn:sendBill\"");

        using var response = await _http.SendAsync(request, ct);
        var cuerpoRespuesta = await response.Content.ReadAsStringAsync(ct);
        var respuesta = XDocument.Parse(cuerpoRespuesta);

        var fault = respuesta.Descendants().FirstOrDefault(e => e.Name.LocalName == "Fault");
        if (fault is not null)
        {
            var faultCode = fault.Elements().FirstOrDefault(e => e.Name.LocalName == "faultcode")?.Value ?? "";
            var faultString = fault.Elements().FirstOrDefault(e => e.Name.LocalName == "faultstring")?.Value ?? "";
            return ResultadoEnvioSunat.Fallo(faultCode, faultString);
        }

        var applicationResponse = respuesta.Descendants().FirstOrDefault(e => e.Name.LocalName == "applicationResponse")
            ?? throw new InvalidOperationException($"Respuesta inesperada de SUNAT (sin Fault ni applicationResponse): {cuerpoRespuesta}");

        return ResultadoEnvioSunat.Aceptado(Convert.FromBase64String(applicationResponse.Value));
    }
}
