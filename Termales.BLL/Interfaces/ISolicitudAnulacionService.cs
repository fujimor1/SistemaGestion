using Termales.Common.DTOs.Comprobante;
using Termales.Common.Wrappers;

namespace Termales.BLL.Interfaces;

public interface ISolicitudAnulacionService
{
    Task<ApiResponse> SolicitarAsync(int comprobanteId, string motivo, string cajero);
    Task<IEnumerable<SolicitudAnulacionDto>> ObtenerPendientesAsync();
    Task<IEnumerable<SolicitudAnulacionDto>> ObtenerHistorialAsync(string? desde, string? hasta);
    Task<ApiResponse> AprobarAsync(int solicitudId, string supervisorNombre);
    Task<ApiResponse> RechazarAsync(int solicitudId, string supervisorNombre, string? motivoRechazo);
}
