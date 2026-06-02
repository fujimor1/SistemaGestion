using Termales.Common.DTOs;
using Termales.Common.Wrappers;

namespace Termales.BLL.Interfaces;

public interface IServicioService
{
    Task<ApiResponse<IEnumerable<ServicioDto>>> ObtenerTodosAsync();
    Task<ApiResponse<IEnumerable<ServicioDto>>> ObtenerActivosAsync();
    Task<ApiResponse<ServicioDto>> ObtenerPorIdAsync(int id);
    Task<ApiResponse<ServicioDto>> CrearAsync(CrearServicioDto dto);
    Task<ApiResponse<ServicioDto>> ActualizarAsync(ActualizarServicioDto dto);
    Task<ApiResponse> CambiarEstadoAsync(int id, bool activo);
}
