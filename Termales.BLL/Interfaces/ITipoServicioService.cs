using Termales.Common.DTOs;
using Termales.Common.Wrappers;

namespace Termales.BLL.Interfaces;

public interface ITipoServicioService
{
    Task<ApiResponse<TipoServicioDto>> ObtenerPorIdAsync(int id);
    Task<ApiResponse<IEnumerable<TipoServicioDto>>> ObtenerTodosAsync();
    Task<ApiResponse<IEnumerable<TipoServicioDto>>> ObtenerActivosAsync();
    Task<ApiResponse<TipoServicioDto>> CrearAsync(CrearTipoServicioDto dto);
    Task<ApiResponse<TipoServicioDto>> ActualizarAsync(ActualizarTipoServicioDto dto);
    Task<ApiResponse> DesactivarAsync(int id);
}
