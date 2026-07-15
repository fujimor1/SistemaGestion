using Termales.BLL.Interfaces;
using Termales.Common.DTOs.Comprobante;
using Termales.Common.Wrappers;
using Termales.DAL.UnitOfWork;
using Termales.Entities.Models;

namespace Termales.BLL.Services;

public class SolicitudAnulacionService : ISolicitudAnulacionService
{
    private readonly IUnitOfWork _uow;
    private readonly INotaCreditoService _notaCredito;

    public SolicitudAnulacionService(IUnitOfWork uow, INotaCreditoService notaCredito)
    {
        _uow = uow;
        _notaCredito = notaCredito;
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

        // La anulación de una Boleta/Factura ya aceptada por SUNAT no es un "borrado": se emite
        // una Nota de Crédito por el total, con motivo "Anulación de la operación". Recién cuando
        // el supervisor aprueba se dispara esto — la solicitud en sí no toca SUNAT para nada.
        if (comprobante.TipoComprobante is "BI" or "FI")
        {
            var dto = new EmitirNotaCreditoDto { Tipo = "total", CodigoMotivo = 1 };
            var resultadoNc = await _notaCredito.EmitirAsync(comprobante.ComprobanteId, dto, supervisorNombre);
            if (!resultadoNc.Exito)
                return ApiResponse.Fallido($"No se pudo emitir la nota de crédito de anulación: {resultadoNc.Mensaje}");
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

        return ApiResponse.Exitoso($"Anulación aprobada. Comprobante {comprobante.Serie}-{comprobante.Numero:D5} anulado.");
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
