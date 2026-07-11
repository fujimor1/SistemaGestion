using Termales.Common.DTOs;
using Termales.Common.Wrappers;

namespace Termales.BLL.Interfaces;

public interface IHabitacionService
{
    Task<ApiResponse<IEnumerable<HabitacionDto>>> ObtenerTodasAsync();
    Task<ApiResponse<HabitacionDto>> ObtenerPorIdAsync(int id);
    Task<ApiResponse<HabitacionDto>> CrearAsync(CrearHabitacionDto dto);
    Task<ApiResponse<HabitacionDto>> ActualizarAsync(ActualizarHabitacionDto dto);
    Task<ApiResponse> CambiarOcupacionAsync(int id, bool ocupado);
    Task<ApiResponse> EliminarAsync(int id);
}
