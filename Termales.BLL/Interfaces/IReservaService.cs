using Termales.Common.DTOs;
using Termales.Common.Helpers;
using Termales.Common.Wrappers;

namespace Termales.BLL.Interfaces;

public interface IReservaService
{
    Task<ApiResponse<ReservaDto>> ObtenerPorIdAsync(int id);
    Task<PagedResponse<ReservaDto>> ObtenerPaginadoAsync(FiltroReserva filtro);
    Task<ApiResponse<IEnumerable<ReservaDto>>> ObtenerPorClienteAsync(int clienteId);
    Task<ApiResponse<ReservaDto>> CrearAsync(CrearReservaDto dto);
    Task<ApiResponse<ReservaDto>> ActualizarEstadoAsync(int reservaId, ActualizarEstadoReservaDto dto);
    Task<ApiResponse> CancelarAsync(int reservaId, string? motivo);
}
