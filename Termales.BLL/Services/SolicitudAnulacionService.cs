using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Termales.BLL.Interfaces;
using Termales.Common.DTOs.Comprobante;
using Termales.Common.Settings;
using Termales.Common.Wrappers;
using Termales.DAL.UnitOfWork;
using Termales.Entities.Models;

namespace Termales.BLL.Services;

public class SolicitudAnulacionService : ISolicitudAnulacionService
{
    private readonly IUnitOfWork      _uow;
    private readonly HttpClient       _nubefactHttp;
    private readonly NubefactSettings _cfg;

    public SolicitudAnulacionService(
        IUnitOfWork uow,
        IHttpClientFactory httpFactory,
        IOptions<NubefactSettings> cfg)
    {
        _uow          = uow;
        _nubefactHttp = httpFactory.CreateClient("Nubefact");
        _cfg          = cfg.Value;
    }

    public async Task<ApiResponse> SolicitarAsync(int comprobanteId, string motivo, string cajero)
    {
        if (string.IsNullOrWhiteSpace(motivo))
            return ApiResponse.Fallido("El motivo de anulación es obligatorio.");

        var comprobante = await _uow.Comprobantes.ObtenerPorIdAsync(comprobanteId);
        if (comprobante is null)
            return ApiResponse.Fallido("Comprobante no encontrado.");
        if (comprobante.Estado == "ANULADO")
            return ApiResponse.Fallido("El comprobante ya está anulado.");
        if (comprobante.Estado == "PENDIENTE DE ANULACIÓN")
            return ApiResponse.Fallido("Ya existe una solicitud de anulación pendiente para este comprobante.");
        if (comprobante.TipoComprobante is not ("NV" or "BI" or "FI"))
            return ApiResponse.Fallido("Solo se pueden anular comprobantes de tipo NV, BI o FI.");

        var solicitud = new SolicitudAnulacion
        {
            ComprobanteId             = comprobanteId,
            Motivo                    = motivo.Trim(),
            SolicitadoPor             = cajero,
            EstadoAnteriorComprobante = comprobante.Estado,
        };

        comprobante.Estado = "PENDIENTE DE ANULACIÓN";
        await _uow.Comprobantes.ActualizarAsync(comprobante);
        await _uow.SolicitudesAnulacion.AgregarAsync(solicitud);
        await _uow.GuardarCambiosAsync();

        return ApiResponse.Exitoso("Solicitud de anulación enviada. El supervisor revisará el motivo.");
    }

    public async Task<IEnumerable<SolicitudAnulacionDto>> ObtenerPendientesAsync()
    {
        var lista = await _uow.SolicitudesAnulacion.ObtenerPendientesAsync();
        return lista.Select(MapDto);
    }

    public async Task<IEnumerable<SolicitudAnulacionDto>> ObtenerHistorialAsync(string? desde, string? hasta)
    {
        var desdeDate = DateOnly.TryParse(desde, out var d) ? d : DateOnly.FromDateTime(DateTime.UtcNow);
        var hastaDate = DateOnly.TryParse(hasta, out var h) ? h : DateOnly.FromDateTime(DateTime.UtcNow);
        var lista = await _uow.SolicitudesAnulacion.ObtenerHistorialAsync(desdeDate, hastaDate);
        return lista.Select(MapDto);
    }

    public async Task<ApiResponse> AprobarAsync(int solicitudId, string supervisorNombre)
    {
        var solicitud = await _uow.SolicitudesAnulacion.ObtenerPorIdAsync(solicitudId);
        if (solicitud is null)            return ApiResponse.Fallido("Solicitud no encontrada.");
        if (solicitud.Estado != "Pendiente") return ApiResponse.Fallido("La solicitud ya fue resuelta.");

        var comprobante = await _uow.Comprobantes.ObtenerPorIdAsync(solicitud.ComprobanteId);
        if (comprobante is null) return ApiResponse.Fallido("Comprobante no encontrado.");

        // Llamada a Nubefact para BI / FI (simulada si ModoSimulacion = true)
        if (comprobante.TipoComprobante is "BI" or "FI" && !_cfg.ModoSimulacion)
        {
            var tipoDoc = comprobante.TipoComprobante == "FI" ? 1 : 2;
            try
            {
                var req = new HttpRequestMessage(HttpMethod.Delete,
                    $"{_cfg.UrlBase.TrimEnd('/')}/{_cfg.Ruc}/comprobantes/{tipoDoc}/{comprobante.Serie}/{comprobante.Numero}");
                req.Headers.Add("Authorization", $"Token {_cfg.Token}");

                var resp = await _nubefactHttp.SendAsync(req);
                if (!resp.IsSuccessStatusCode)
                {
                    var err = await resp.Content.ReadAsStringAsync();
                    return ApiResponse.Fallido($"Error al anular en Nubefact: {err}");
                }
            }
            catch (HttpRequestException ex)
            {
                return ApiResponse.Fallido($"Error al conectar con Nubefact: {ex.Message}");
            }
        }

        comprobante.Estado          = "ANULADO";
        comprobante.MotivoAnulacion = solicitud.Motivo;
        comprobante.AutorizadoPor   = supervisorNombre;
        await _uow.Comprobantes.ActualizarAsync(comprobante);

        solicitud.Estado          = "Aprobada";
        solicitud.ResueltoPor     = supervisorNombre;
        solicitud.FechaResolucion = DateTime.UtcNow;
        await _uow.SolicitudesAnulacion.ActualizarAsync(solicitud);

        await _uow.GuardarCambiosAsync();

        var modoLabel = _cfg.ModoSimulacion ? " (simulación)" : "";
        return ApiResponse.Exitoso($"Anulación aprobada{modoLabel}. Comprobante {comprobante.Serie}-{comprobante.Numero:D5} anulado.");
    }

    public async Task<ApiResponse> RechazarAsync(int solicitudId, string supervisorNombre, string? motivoRechazo)
    {
        var solicitud = await _uow.SolicitudesAnulacion.ObtenerPorIdAsync(solicitudId);
        if (solicitud is null)              return ApiResponse.Fallido("Solicitud no encontrada.");
        if (solicitud.Estado != "Pendiente") return ApiResponse.Fallido("La solicitud ya fue resuelta.");

        var comprobante = await _uow.Comprobantes.ObtenerPorIdAsync(solicitud.ComprobanteId);
        if (comprobante is null) return ApiResponse.Fallido("Comprobante no encontrado.");

        // Revertir el comprobante al estado anterior
        comprobante.Estado = solicitud.EstadoAnteriorComprobante;
        await _uow.Comprobantes.ActualizarAsync(comprobante);

        solicitud.Estado          = "Rechazada";
        solicitud.ResueltoPor     = supervisorNombre;
        solicitud.FechaResolucion = DateTime.UtcNow;
        solicitud.MotivoRechazo   = motivoRechazo?.Trim();
        await _uow.SolicitudesAnulacion.ActualizarAsync(solicitud);

        await _uow.GuardarCambiosAsync();

        return ApiResponse.Exitoso("Solicitud rechazada. El comprobante vuelve a su estado anterior.");
    }

    private static SolicitudAnulacionDto MapDto(SolicitudAnulacion s) => new()
    {
        SolicitudAnulacionId = s.SolicitudAnulacionId,
        ComprobanteId        = s.ComprobanteId,
        NumeroFormateado     = $"{s.Comprobante.Serie}-{s.Comprobante.Numero:D5}",
        TipoComprobante      = s.Comprobante.TipoComprobante,
        TipoAmbiente         = s.Comprobante.TipoAmbiente,
        ClienteNombre        = s.Comprobante.ClienteNombre ?? s.Comprobante.ClienteRazonSocial,
        Total                = s.Comprobante.Total,
        Motivo               = s.Motivo,
        SolicitadoPor        = s.SolicitadoPor,
        FechaSolicitud       = s.FechaSolicitud,
        Estado               = s.Estado,
        ResueltoPor          = s.ResueltoPor,
        FechaResolucion      = s.FechaResolucion,
        MotivoRechazo        = s.MotivoRechazo,
    };
}
