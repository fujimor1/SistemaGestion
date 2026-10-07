using Termales.Common.DTOs;
using Termales.Common.Wrappers;

namespace Termales.BLL.Interfaces;

public interface IPaqueteBanioService
{
    Task<ApiResponse<IEnumerable<PaqueteBanioDto>>> ObtenerActivosAsync();
    Task<ApiResponse<IEnumerable<PaqueteBanioDto>>> ObtenerTodosAsync();
    Task<ApiResponse<PaqueteBanioDto>> CrearAsync(CrearPaqueteBanioDto dto);
    Task<ApiResponse<PaqueteBanioDto>> ActualizarAsync(ActualizarPaqueteBanioDto dto);
    Task<ApiResponse> DesactivarAsync(int id);
    Task<ApiResponse> ActivarAsync(int id);
}
