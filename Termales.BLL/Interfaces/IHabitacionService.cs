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
    Task<ApiResponse> MarcarLimpiaAsync(int id);
    Task<ApiResponse> EliminarAsync(int id);
    Task<ApiResponse> ReordenarAsync(ReordenarHabitacionesDto dto);

    Task<ApiResponse<IEnumerable<HabitacionItemDto>>> ObtenerItemsAsync(int habitacionId);
    Task<ApiResponse<HabitacionItemDto>> AgregarItemAsync(int habitacionId, CrearHabitacionItemDto dto);
    Task<ApiResponse> EliminarItemAsync(int habitacionItemId);
}
