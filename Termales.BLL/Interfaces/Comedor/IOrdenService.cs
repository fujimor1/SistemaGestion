using Termales.Common.DTOs.Comedor;
using Termales.Common.Wrappers;
using Termales.Entities.Enums;

namespace Termales.BLL.Interfaces.Comedor;

public interface IOrdenService
{
    Task<ApiResponse<OrdenDto>> ObtenerPorIdAsync(int id);
    Task<ApiResponse<OrdenDto>> ObtenerActivaPorMesaAsync(int mesaId);
    Task<ApiResponse<IEnumerable<OrdenDto>>> ObtenerPorEstadoAsync(EstadoOrden estado);
    Task<ApiResponse<IEnumerable<OrdenDto>>> ObtenerPorFechaAsync(DateTime fecha);
    Task<ApiResponse<OrdenDto>> CrearAsync(CrearOrdenDto dto);
    Task<ApiResponse<OrdenDto>> AgregarItemsAsync(int ordenId, AgregarItemsOrdenDto dto);
    Task<ApiResponse<OrdenDetalleDto>> ActualizarEstadoDetalleAsync(int detalleId, ActualizarEstadoDetalleDto dto);
    Task<ApiResponse<OrdenDto>> MarcarListaAsync(int ordenId);
    Task<ApiResponse<OrdenDto>> PasarACajaAsync(int ordenId);
    Task<ApiResponse<OrdenDto>> CerrarOrdenAsync(int ordenId);
    Task<ApiResponse> CancelarAsync(int ordenId, string motivo);
    Task<ApiResponse<OrdenDto>> EliminarDetalleAsync(int detalleId);
}
