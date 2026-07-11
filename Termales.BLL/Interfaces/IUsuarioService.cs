using Termales.Common.DTOs;
using Termales.Common.Wrappers;

namespace Termales.BLL.Interfaces;

public interface IUsuarioService
{
    Task<ApiResponse<IEnumerable<UsuarioDto>>> ObtenerTodosAsync();
    Task<ApiResponse<UsuarioDto>> ObtenerPorIdAsync(int id);
    Task<ApiResponse<UsuarioDto>> CrearAsync(CrearUsuarioDto dto);
    Task<ApiResponse<UsuarioDto>> ActualizarAsync(ActualizarUsuarioDto dto);
    Task<ApiResponse> CambiarPasswordAsync(int id, CambiarPasswordDto dto);
    Task<ApiResponse> DesactivarAsync(int id);
    Task<ApiResponse<IEnumerable<RolDto>>> ObtenerRolesAsync();
}
