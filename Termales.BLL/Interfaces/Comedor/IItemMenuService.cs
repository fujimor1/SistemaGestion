using Termales.Common.DTOs.Comedor;
using Termales.Common.Wrappers;

namespace Termales.BLL.Interfaces.Comedor;

public interface IItemMenuService
{
    Task<ApiResponse<IEnumerable<ItemMenuDto>>> ObtenerTodosActivosAsync();
    Task<ApiResponse<IEnumerable<ItemMenuDto>>> ObtenerPorCategoriaAsync(int categoriaId);
    Task<ApiResponse<ItemMenuDto>> ObtenerPorIdAsync(int id);
    Task<ApiResponse<ItemMenuDto>> CrearAsync(CrearItemMenuDto dto);
    Task<ApiResponse<ItemMenuDto>> ActualizarAsync(ActualizarItemMenuDto dto);
    Task<ApiResponse> DesactivarAsync(int id);
}
