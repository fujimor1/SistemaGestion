using Termales.Common.DTOs.Comedor;
using Termales.Common.Wrappers;

namespace Termales.BLL.Interfaces.Comedor;

public interface ICategoriaMenuService
{
    Task<ApiResponse<IEnumerable<CategoriaMenuDto>>> ObtenerTodosAsync();
    Task<ApiResponse<CategoriaMenuDto>> ObtenerPorIdAsync(int id);
    Task<ApiResponse<CategoriaMenuDto>> CrearAsync(CrearCategoriaMenuDto dto);
    Task<ApiResponse<CategoriaMenuDto>> ActualizarAsync(ActualizarCategoriaMenuDto dto);
    Task<ApiResponse> DesactivarAsync(int id);
}
