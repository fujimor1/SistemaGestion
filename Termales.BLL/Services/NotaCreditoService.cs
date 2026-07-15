using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Termales.BLL.Interfaces;
using Termales.BLL.Interfaces.Sunat;
using Termales.Common.DTOs.Comprobante;
using Termales.Common.Settings;
using Termales.Common.Wrappers;
using Termales.DAL.UnitOfWork;
using Termales.Entities.Models;

namespace Termales.BLL.Services;

public class NotaCreditoService : INotaCreditoService
{
    private readonly IUnitOfWork _uow;
    private readonly HttpClient _nubefactHttp;
    private readonly NubefactSettings _cfg;
    private readonly IFacturaElectronicaService _facturaElectronica;
    private readonly SunatSettings _sunatCfg;

    public NotaCreditoService(
        IUnitOfWork uow,
        IHttpClientFactory httpFactory,
        IOptions<NubefactSettings> cfg,
        IFacturaElectronicaService facturaElectronica,
        IOptions<SunatSettings> sunatCfg)
    {
        _uow = uow;
        _nubefactHttp = httpFactory.CreateClient("Nubefact");
        _cfg = cfg.Value;
        _facturaElectronica = facturaElectronica;
        _sunatCfg = sunatCfg.Value;
    }

    public async Task<ApiResponse<ComprobanteResultadoDto>> EmitirAsync(
        int comprobanteOrigenId, EmitirNotaCreditoDto dto, string cajero)
    {
        var origen = await _uow.Comprobantes.ObtenerPorIdAsync(comprobanteOrigenId);
        if (origen is null)
            return ApiResponse<ComprobanteResultadoDto>.Fallido("Comprobante original no encontrado");
        if (origen.TipoComprobante is not ("BI" or "FI"))
            return ApiResponse<ComprobanteResultadoDto>.Fallido("Solo se pueden emitir notas de crédito para Boletas o Facturas");
        if (origen.Estado == "ANULADO")
            return ApiResponse<ComprobanteResultadoDto>.Fallido("No se puede emitir nota de crédito sobre un comprobante anulado");

        decimal montoNc;
        if (dto.Tipo == "parcial")
        {
            if (dto.MontoDevolucion is null or <= 0)
                return ApiResponse<ComprobanteResultadoDto>.Fallido("Debe indicar el monto de devolución para nota de crédito parcial");
            if (dto.MontoDevolucion > origen.Total)
                return ApiResponse<ComprobanteResultadoDto>.Fallido("El monto de devolución no puede superar el total del comprobante original");
            montoNc = dto.MontoDevolucion.Value;
        }
        else
        {
            montoNc = origen.Total;
        }

        return _sunatCfg.Habilitado
            ? await EmitirDirectoSunat(origen, dto, montoNc, cajero)
            : await EmitirConNubefact(origen, dto, montoNc, cajero);
    }

    // ── Nota de crédito directa a SUNAT (sin Nubefact) ────────────────
    private async Task<ApiResponse<ComprobanteResultadoDto>> EmitirDirectoSunat(
        Comprobante origen, EmitirNotaCreditoDto dto, decimal montoNc, string cajero)
    {
        var esBoleta  = origen.TipoComprobante == "BI";
        var serieNc   = esBoleta ? _sunatCfg.SerieNcBoleta : _sunatCfg.SerieNcFactura;
        var gravadaNc = Math.Round(montoNc / 1.18m, 2);
        var igvNc     = Math.Round(montoNc - gravadaNc, 2);
        var numero    = await _uow.ComprobanteSeries.SiguienteNumeroAsync(serieNc, "NC");
        var (codigoMotivo, motivoTexto) = ObtenerMotivoNc(dto.CodigoMotivo);
        var descripcionLinea = dto.Tipo == "parcial"
            ? $"Devolución parcial - {origen.Serie}-{origen.Numero:D5}"
            : $"Anulación total - {origen.Serie}-{origen.Numero:D5}";

        var nc = new Comprobante
        {
            Serie               = serieNc,
            Numero              = numero,
            TipoComprobante     = "NC",
            TipoAmbiente        = origen.TipoAmbiente,
            ReferenciaId        = origen.ReferenciaId,
            ComprobanteOrigenId = origen.ComprobanteId,
            ComprobanteOrigen   = origen,
            ClienteDni          = origen.ClienteDni,
            ClienteRuc          = origen.ClienteRuc,
            ClienteNombre       = origen.ClienteNombre,
            ClienteRazonSocial  = origen.ClienteRazonSocial,
            Cajero              = cajero,
            TotalGravada        = gravadaNc,
            Impuesto            = igvNc,
            Total               = montoNc,
            Estado              = "PENDIENTE DE ENVÍO A SUNAT",
            EnlacePdf           = "",
            CodigoMotivoNc      = codigoMotivo,
            MotivoAnulacion     = motivoTexto,
            Detalles = new List<ComprobanteDetalle>
            {
                new() { Descripcion = descripcionLinea, Cantidad = 1, PrecioUnitario = montoNc, Subtotal = montoNc },
            },
        };
        await _uow.Comprobantes.AgregarAsync(nc);
        await _uow.GuardarCambiosAsync();

        var resultadoSunat = await _facturaElectronica.EmitirAsync(nc);
        nc.Estado = resultadoSunat.Exito
            ? (resultadoSunat.Data!.Aceptado ? "ENVIADO A SUNAT" : "RECHAZADO POR SUNAT")
            : "PENDIENTE DE ENVÍO A SUNAT";
        await _uow.Comprobantes.ActualizarAsync(nc);
        await _uow.GuardarCambiosAsync();

        var tipoLabelDirecto = dto.Tipo == "parcial" ? "parcial" : "total";
        return ApiResponse<ComprobanteResultadoDto>.Exitoso(new ComprobanteResultadoDto
        {
            ComprobanteId    = nc.ComprobanteId,
            TipoComprobante  = "NC",
            Ambiente         = origen.TipoAmbiente,
            Serie            = serieNc,
            Numero           = numero,
            NumeroFormateado = $"{serieNc}-{numero:D5}",
            Cajero           = cajero,
            TotalGravada     = gravadaNc,
            Impuesto         = igvNc,
            Total            = montoNc,
            Estado           = nc.Estado,
            EnlacePdf        = "",
            ModoSimulacion   = false,
        }, resultadoSunat.Exito
            ? resultadoSunat.Mensaje
            : $"Nota de crédito {tipoLabelDirecto} registrada; pendiente de envío a SUNAT ({resultadoSunat.Mensaje})");
    }

    private static (string Codigo, string Descripcion) ObtenerMotivoNc(int codigoMotivo) => codigoMotivo switch
    {
        1 => ("01", "ANULACION DE LA OPERACION"),
        2 => ("02", "ANULACION POR ERROR EN EL RUC"),
        3 => ("03", "CORRECCION POR ERROR EN LA DESCRIPCION"),
        4 => ("04", "DESCUENTO GLOBAL"),
        5 => ("05", "DESCUENTO POR ITEM"),
        6 => ("06", "DEVOLUCION TOTAL"),
        _ => ("01", "ANULACION DE LA OPERACION"),
    };

    // ── Nota de crédito vía Nubefact (legado) ──────────────────────────
    private async Task<ApiResponse<ComprobanteResultadoDto>> EmitirConNubefact(
        Comprobante origen, EmitirNotaCreditoDto dto, decimal montoNc, string cajero)
    {
        var esBoleta  = origen.TipoComprobante == "BI";
        var tipoNc    = esBoleta ? 7 : 8;
        var serieNc   = esBoleta ? _cfg.SerieNcBoleta : _cfg.SerieNcFactura;

        var gravadaNc = Math.Round(montoNc / 1.18m, 2);
        var igvNc     = Math.Round(montoNc - gravadaNc, 2);
        var numero    = await _uow.ComprobanteSeries.SiguienteNumeroAsync(serieNc, "NC");

        string enlacePdf;

        if (_cfg.ModoSimulacion)
        {
            enlacePdf = string.Empty;
        }
        else
        {
            var clienteTipo = esBoleta ? (string.IsNullOrWhiteSpace(origen.ClienteDni) ? 0 : 1) : 6;
            var clienteDoc  = esBoleta ? origen.ClienteDni ?? "" : origen.ClienteRuc ?? "";
            var clienteNom  = esBoleta ? origen.ClienteNombre ?? "CLIENTES VARIOS" : origen.ClienteRazonSocial ?? "";
            var descripcionNc = dto.Tipo == "parcial"
                ? $"Devolución parcial - {origen.Serie}-{origen.Numero:D5}"
                : $"Anulación total - {origen.Serie}-{origen.Numero:D5}";

            var payload = new
            {
                operacion           = "generar_comprobante",
                tipo_de_comprobante = tipoNc,
                serie               = serieNc,
                numero,
                tipo_de_nota_de_credito = dto.CodigoMotivo,
                nota_credito_o_debito_serie_comprobante_afectado  = origen.Serie,
                nota_credito_o_debito_numero_comprobante_afectado = origen.Numero.ToString(),
                sunat_transaction   = 3,
                cliente_tipo_de_documento   = clienteTipo,
                cliente_numero_de_documento = clienteDoc,
                cliente_denominacion        = clienteNom,
                cliente_direccion    = "",
                cliente_email        = "",
                fecha_de_emision     = DateTime.Now.ToString("dd/MM/yyyy"),
                moneda               = 1,
                porcentaje_de_igv    = 18,
                total_gravada        = gravadaNc,
                total_igv            = igvNc,
                total                = montoNc,
                enviar_automaticamente_a_la_sunat = true,
                enviar_automaticamente_al_cliente = false,
                items = new[]
                {
                    new
                    {
                        unidad_de_medida  = "ZZ",
                        descripcion       = descripcionNc,
                        cantidad          = 1,
                        valor_unitario    = gravadaNc,
                        precio_unitario   = montoNc,
                        subtotal          = gravadaNc,
                        igv               = igvNc,
                        total             = montoNc,
                        tipo_de_igv       = 1,
                        anticipo_regularizacion   = false,
                        anticipo_documento_serie  = "",
                        anticipo_documento_numero = "",
                    }
                }
            };

            try
            {
                var req = new HttpRequestMessage(HttpMethod.Post,
                    $"{_cfg.UrlBase.TrimEnd('/')}/{_cfg.Ruc}/comprobantes")
                { Content = JsonContent.Create(payload) };
                req.Headers.Add("Authorization", $"Token {_cfg.Token}");

                var resp = await _nubefactHttp.SendAsync(req);
                var json = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode)
                    return ApiResponse<ComprobanteResultadoDto>.Fallido($"Error Nubefact: {json}");

                using var doc = JsonDocument.Parse(json);
                enlacePdf = doc.RootElement.TryGetProperty("enlace_del_pdf", out var p) ? p.GetString() ?? "" : "";
            }
            catch (Exception ex)
            {
                return ApiResponse<ComprobanteResultadoDto>.Fallido($"Error al conectar con Nubefact: {ex.Message}");
            }
        }

        var estado = _cfg.ModoSimulacion ? "SIMULADO" : "ENVIADO A SUNAT";

        var nc = new Comprobante
        {
            Serie                = serieNc,
            Numero               = numero,
            TipoComprobante      = "NC",
            TipoAmbiente         = origen.TipoAmbiente,
            ReferenciaId         = origen.ReferenciaId,
            ComprobanteOrigenId  = origen.ComprobanteId,
            ClienteDni           = origen.ClienteDni,
            ClienteRuc           = origen.ClienteRuc,
            ClienteNombre        = origen.ClienteNombre,
            ClienteRazonSocial   = origen.ClienteRazonSocial,
            Cajero               = cajero,
            TotalGravada         = gravadaNc,
            Impuesto             = igvNc,
            Total                = montoNc,
            Estado               = estado,
            EnlacePdf            = enlacePdf,
        };
        await _uow.Comprobantes.AgregarAsync(nc);
        await _uow.GuardarCambiosAsync();

        var tipoLabel = dto.Tipo == "parcial" ? "parcial" : "total";
        var msg = _cfg.ModoSimulacion
            ? $"Nota de crédito {tipoLabel} simulada"
            : $"Nota de crédito {tipoLabel} enviada a SUNAT";

        return ApiResponse<ComprobanteResultadoDto>.Exitoso(new ComprobanteResultadoDto
        {
            ComprobanteId    = nc.ComprobanteId,
            TipoComprobante  = "NC",
            Ambiente         = origen.TipoAmbiente,
            Serie            = serieNc,
            Numero           = numero,
            NumeroFormateado = $"{serieNc}-{numero:D5}",
            Cajero           = cajero,
            TotalGravada     = gravadaNc,
            Impuesto         = igvNc,
            Total            = montoNc,
            Estado           = estado,
            EnlacePdf        = enlacePdf,
            ModoSimulacion   = _cfg.ModoSimulacion,
        }, msg);
    }
}
