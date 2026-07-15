using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Options;
using Termales.BLL.Interfaces.Sunat;
using Termales.BLL.Services.Sunat.Cdr;
using Termales.BLL.Services.Sunat.Empaquetado;
using Termales.BLL.Services.Sunat.Firma;
using Termales.BLL.Services.Sunat.Soap;
using Termales.BLL.Services.Sunat.Xml;
using Termales.Common.Settings;
using Termales.Entities.Models;

namespace Termales.Tests.Sunat;

/// <summary>
/// Pruebas de integración REALES contra el ambiente BETA de SUNAT, usando las credenciales públicas
/// de prueba (RUC 20000000001 / MODDATOS / moddatos) y un certificado autofirmado desechable
/// (Beta no exige un certificado registrado en SUNAT). Requieren conexión a internet.
/// </summary>
public class SunatBillServiceClientTests : IDisposable
{
    private const string PfxPassword = "Test1234!";
    private readonly string _pfxPath = Path.Combine(Path.GetTempPath(), $"collpa-test-cert-{Guid.NewGuid():N}.pfx");

    private static readonly EmpresaSettings EmpresaPrueba = new()
    {
        RazonSocial = "EMPRESA DE PRUEBA SAC",
        Ruc = "20000000001",
        Direccion = "AV. PRUEBA 123",
        Urbanizacion = "-",
        Distrito = "LIMA",
        Provincia = "LIMA",
        Departamento = "LIMA",
        Ubigeo = "150101",
        CodigoPais = "PE",
        CodigoEstablecimiento = "0000",
    };

    public SunatBillServiceClientTests()
    {
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest("CN=Collpa Test", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        using var cert = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(1));
        File.WriteAllBytes(_pfxPath, cert.Export(X509ContentType.Pfx, PfxPassword));
    }

    public void Dispose() => File.Delete(_pfxPath);

    [Fact]
    public async Task EnviarAsync_Factura_ContraBetaConCredencialesPublicas_DevuelveCdrAceptado()
    {
        var comprobante = new Comprobante
        {
            Serie = "F001",
            Numero = new Random().Next(1, 99_999_999), // Beta no exige correlativo consecutivo real
            TipoComprobante = "FI",
            ClienteRuc = "20123456789",
            ClienteRazonSocial = "Cliente de Prueba SAC",
            Moneda = "PEN",
            TotalGravada = 84.75m,
            Impuesto = 15.25m,
            Total = 100.00m,
            FechaEmision = DateTime.UtcNow,
        };
        comprobante.Detalles.Add(new ComprobanteDetalle
        {
            Descripcion = "Servicio de prueba - Factura",
            Cantidad = 1,
            PrecioUnitario = 100.00m,
            Subtotal = 100.00m,
        });

        var cdr = await EnviarYObtenerCdrAsync(comprobante, "01");
        Assert.Equal(0, cdr.Codigo);
    }

    [Fact]
    public async Task EnviarAsync_Boleta_ContraBetaConCredencialesPublicas_DevuelveCdrAceptado()
    {
        var comprobante = new Comprobante
        {
            Serie = "B001",
            Numero = new Random().Next(1, 99_999_999),
            TipoComprobante = "BI",
            ClienteDni = "12345678",
            ClienteNombre = "Cliente de Prueba",
            Moneda = "PEN",
            TotalGravada = 84.75m,
            Impuesto = 15.25m,
            Total = 100.00m,
            FechaEmision = DateTime.UtcNow,
        };
        comprobante.Detalles.Add(new ComprobanteDetalle
        {
            Descripcion = "Servicio de prueba - Boleta",
            Cantidad = 1,
            PrecioUnitario = 100.00m,
            Subtotal = 100.00m,
        });

        var cdr = await EnviarYObtenerCdrAsync(comprobante, "03");
        Assert.Equal(0, cdr.Codigo);
    }

    [Fact]
    public async Task EnviarAsync_BoletaSinDocumentoDelCliente_ContraBetaConCredencialesPublicas_DevuelveCdrAceptado()
    {
        var comprobante = new Comprobante
        {
            Serie = "B001",
            Numero = new Random().Next(1, 99_999_999),
            TipoComprobante = "BI",
            ClienteDni = null,
            ClienteNombre = null,
            Moneda = "PEN",
            TotalGravada = 84.75m,
            Impuesto = 15.25m,
            Total = 100.00m,
            FechaEmision = DateTime.UtcNow,
        };
        comprobante.Detalles.Add(new ComprobanteDetalle
        {
            Descripcion = "Servicio de prueba - Boleta sin documento",
            Cantidad = 1,
            PrecioUnitario = 100.00m,
            Subtotal = 100.00m,
        });

        var cdr = await EnviarYObtenerCdrAsync(comprobante, "03");
        Assert.Equal(0, cdr.Codigo);
    }

    [Fact(Skip = "El backend de SUNAT Beta para Notas de Crédito (tipo 07) está devolviendo " +
        "HTTP 500 con header 'X-Backside-Transport: FAIL FAIL' (falla de su propio gateway, " +
        "no un rechazo de nuestro XML/firma) — Factura/Boleta siguen aceptándose con normalidad " +
        "en el mismo momento. Reactivar este test cuando se confirme que el servicio de Beta " +
        "para NC volvió a responder.")]
    public async Task EnviarAsync_NotaCreditoDeFactura_ContraBetaConCredencialesPublicas_DevuelveCdrAceptado()
    {
        var factura = new Comprobante
        {
            Serie = "F001",
            Numero = new Random().Next(1, 99_999_999),
            TipoComprobante = "FI",
            ClienteRuc = "20123456789",
            ClienteRazonSocial = "Cliente de Prueba SAC",
            Moneda = "PEN",
            TotalGravada = 84.75m,
            Impuesto = 15.25m,
            Total = 100.00m,
            FechaEmision = DateTime.UtcNow,
        };
        factura.Detalles.Add(new ComprobanteDetalle
        {
            Descripcion = "Servicio de prueba - Factura a anular",
            Cantidad = 1,
            PrecioUnitario = 100.00m,
            Subtotal = 100.00m,
        });

        var cdrFactura = await EnviarYObtenerCdrAsync(factura, "01");
        Assert.Equal(0, cdrFactura.Codigo);

        var notaCredito = new Comprobante
        {
            Serie = "FC01",
            Numero = new Random().Next(1, 99_999_999),
            TipoComprobante = "NC",
            ComprobanteOrigenId = 1,
            ComprobanteOrigen = factura,
            ClienteRuc = factura.ClienteRuc,
            ClienteRazonSocial = factura.ClienteRazonSocial,
            Moneda = "PEN",
            TotalGravada = factura.TotalGravada,
            Impuesto = factura.Impuesto,
            Total = factura.Total,
            CodigoMotivoNc = "01",
            MotivoAnulacion = "ANULACION DE LA OPERACION",
            FechaEmision = DateTime.UtcNow,
        };
        notaCredito.Detalles.Add(new ComprobanteDetalle
        {
            Descripcion = $"Anulación total - {factura.Serie}-{factura.Numero}",
            Cantidad = 1,
            PrecioUnitario = factura.Total,
            Subtotal = factura.Total,
        });

        var xmlNc = new NotaCreditoXmlBuilder().Construir(notaCredito, factura, EmpresaPrueba);
        var cdrNc = await EnviarXmlYObtenerCdrAsync(xmlNc, "07", notaCredito.Serie, notaCredito.Numero);
        Assert.Equal(0, cdrNc.Codigo);
    }

    private async Task<ResultadoCdr> EnviarYObtenerCdrAsync(Comprobante comprobante, string tipoDoc)
    {
        var xmlSinFirmar = new FacturaXmlBuilder().Construir(comprobante, EmpresaPrueba);
        return await EnviarXmlYObtenerCdrAsync(xmlSinFirmar, tipoDoc, comprobante.Serie, comprobante.Numero);
    }

    private async Task<ResultadoCdr> EnviarXmlYObtenerCdrAsync(
        System.Xml.Linq.XDocument xmlSinFirmar, string tipoDoc, string serie, int numero)
    {
        var sunatCfg = Options.Create(new SunatSettings
        {
            Ruc = "20000000001",
            UsuarioSol = "MODDATOS",
            PasswordSol = "moddatos",
            CertificadoPath = _pfxPath,
            CertificadoPassword = PfxPassword,
            Ambiente = "Beta",
        });

        var firma = new XmlDsigSigner(sunatCfg).Firmar(xmlSinFirmar);
        var zip = new ComprobanteZipBuilder().Construir(EmpresaPrueba.Ruc, tipoDoc, serie, numero, firma.XmlFirmado);

        using var http = new HttpClient();
        var cliente = new SunatBillServiceClient(http, sunatCfg);
        var resultado = await cliente.EnviarAsync(zip.NombreArchivo, zip.ContenidoZip);

        Assert.True(resultado.Exito, $"SUNAT rechazó el envío: {resultado.FaultCode} - {resultado.FaultString}");
        Assert.NotNull(resultado.CdrZip);

        return new CdrParser().Parsear(resultado.CdrZip!);
    }
}
