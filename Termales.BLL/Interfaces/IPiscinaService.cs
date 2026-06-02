using Termales.Common.DTOs;
using Termales.Common.Wrappers;

namespace Termales.BLL.Interfaces;

public interface IPiscinaService
{
    Task<ApiResponse<IEnumerable<PiscinaDto>>> ObtenerTodasAsync();
    Task<ApiResponse<IEnumerable<PiscinaDto>>> ObtenerDisponiblesAsync();
    Task<ApiResponse<IEnumerable<PiscinaDto>>> ObtenerDisponiblesEnFechaAsync(DateTime ingreso, DateTime salida);
    Task<ApiResponse<PiscinaDto>> ObtenerPorIdAsync(int id);
    Task<ApiResponse<PiscinaDto>> CrearAsync(CrearPiscinaDto dto);
    Task<ApiResponse<PiscinaDto>> ActualizarAsync(ActualizarPiscinaDto dto);
    Task<ApiResponse> CambiarDisponibilidadAsync(int id, bool disponible);
}
