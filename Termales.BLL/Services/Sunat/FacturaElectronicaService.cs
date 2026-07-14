using Microsoft.Extensions.Options;
using Termales.BLL.Interfaces.Sunat;
using Termales.Common.DTOs.Sunat;
using Termales.Common.Settings;
using Termales.Common.Wrappers;
using Termales.DAL.UnitOfWork;
using Termales.Entities.Enums;
using Termales.Entities.Models;

namespace Termales.BLL.Services.Sunat;

/// <summary>Orquesta XML → firma → PDF → ZIP → envío → CDR → persistencia, para Factura/Boleta directa a SUNAT.</summary>
public class FacturaElectronicaService : IFacturaElectronicaService
{
    private const string TipoDocFactura = "01"; // catálogo 01
    private const string TipoDocBoleta = "03";  // catálogo 01

    private readonly IUnitOfWork _uow;
    private readonly IFacturaXmlBuilder _xmlBuilder;
    private readonly IXmlDsigSigner _signer;
    private readonly IComprobanteZipBuilder _zipBuilder;
    private readonly ISunatBillServiceClient _billServiceClient;
    private readonly ICdrParser _cdrParser;
    private readonly IRepresentacionImpresaBuilder _representacionImpresaBuilder;
    private readonly SunatSettings _sunatCfg;
    private readonly EmpresaSettings _empresaCfg;

    public FacturaElectronicaService(
        IUnitOfWork uow,
        IFacturaXmlBuilder xmlBuilder,
        IXmlDsigSigner signer,
        IComprobanteZipBuilder zipBuilder,
        ISunatBillServiceClient billServiceClient,
        ICdrParser cdrParser,
        IRepresentacionImpresaBuilder representacionImpresaBuilder,
        IOptions<SunatSettings> sunatCfg,
        IOptions<EmpresaSettings> empresaCfg)
    {
        _uow = uow;
        _xmlBuilder = xmlBuilder;
        _signer = signer;
        _zipBuilder = zipBuilder;
        _billServiceClient = billServiceClient;
        _cdrParser = cdrParser;
        _representacionImpresaBuilder = representacionImpresaBuilder;
        _sunatCfg = sunatCfg.Value;
        _empresaCfg = empresaCfg.Value;
    }

    public async Task<ApiResponse<ResultadoEmisionSunatDto>> EmitirAsync(Comprobante comprobante)
    {
        var tipoDoc = comprobante.TipoComprobante == "FI" ? TipoDocFactura : TipoDocBoleta;
        var xmlSinFirmar = _xmlBuilder.Construir(comprobante, _empresaCfg);
        var firma = _signer.Firmar(xmlSinFirmar);
        var pdf = _representacionImpresaBuilder.Generar(comprobante, _empresaCfg, firma.DigestValueBase64);
        var zip = _zipBuilder.Construir(_sunatCfg.Ruc, tipoDoc, comprobante.Serie, comprobante.Numero, firma.XmlFirmado);

        var comprobanteSunat = await _uow.ComprobantesSunat.ObtenerPorComprobanteIdAsync(comprobante.ComprobanteId);
        var esNuevo = comprobanteSunat is null;
        comprobanteSunat ??= new ComprobanteSunat
        {
            ComprobanteId = comprobante.ComprobanteId,
            FechaLimiteEnvio = comprobante.FechaEmision.AddDays(3), // plazo legal de envío a SUNAT
        };

        comprobanteSunat.XmlFirmado = firma.XmlFirmado;
        comprobanteSunat.HashDigestValue = firma.DigestValueBase64;
        comprobanteSunat.IntentosEnvio += 1;

        try
        {
            var resultadoEnvio = await _billServiceClient.EnviarAsync(zip.NombreArchivo, zip.ContenidoZip);

            if (!resultadoEnvio.Exito)
            {
                comprobanteSunat.Estado = EstadoEnvioSunat.ErrorEnvio;
                comprobanteSunat.CdrDescripcion = $"{resultadoEnvio.FaultCode}: {resultadoEnvio.FaultString}";
                await GuardarAsync(comprobanteSunat, esNuevo);
                return ApiResponse<ResultadoEmisionSunatDto>.Fallido($"SUNAT rechazó el envío: {comprobanteSunat.CdrDescripcion}");
            }

            var cdr = _cdrParser.Parsear(resultadoEnvio.CdrZip!);
            comprobanteSunat.CdrCodigoRespuesta = cdr.Codigo;
            comprobanteSunat.CdrDescripcion = cdr.Descripcion;
            comprobanteSunat.Estado = cdr.Codigo == 0 ? EstadoEnvioSunat.Aceptado : EstadoEnvioSunat.Rechazado;
            comprobanteSunat.FechaEnvioSunat = DateTime.UtcNow;
            await GuardarAsync(comprobanteSunat, esNuevo);

            return ApiResponse<ResultadoEmisionSunatDto>.Exitoso(new ResultadoEmisionSunatDto
            {
                Aceptado = cdr.Codigo == 0,
                CdrCodigo = cdr.Codigo,
                CdrDescripcion = cdr.Descripcion,
                RepresentacionImpresaPdf = pdf,
            }, cdr.Codigo == 0 ? "Factura aceptada por SUNAT" : "SUNAT observó/rechazó el comprobante");
        }
        catch (Exception ex)
        {
            // Error de red/timeout, no un rechazo de negocio — la venta ya quedó registrada
            // localmente, el correlativo no se pierde, y esto se puede reintentar (reenviar-sunat).
            comprobanteSunat.Estado = EstadoEnvioSunat.ErrorEnvio;
            comprobanteSunat.CdrDescripcion = $"Error de conexión con SUNAT: {ex.Message}";
            await GuardarAsync(comprobanteSunat, esNuevo);
            return ApiResponse<ResultadoEmisionSunatDto>.Fallido(comprobanteSunat.CdrDescripcion);
        }
    }

    private async Task GuardarAsync(ComprobanteSunat comprobanteSunat, bool esNuevo)
    {
        if (esNuevo)
            await _uow.ComprobantesSunat.AgregarAsync(comprobanteSunat);
        else
            await _uow.ComprobantesSunat.ActualizarAsync(comprobanteSunat);
        await _uow.GuardarCambiosAsync();
    }
}
