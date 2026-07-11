using Termales.Common.DTOs.Auth;
using Termales.Common.Wrappers;

namespace Termales.BLL.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto dto);
    ApiResponse LogoutAsync(string jti, DateTime expiracionToken);
    /// <summary>Valida credenciales y que el usuario tenga rol Supervisor o Administrador. Devuelve el nombre completo si es válido, null si falla.</summary>
    Task<string?> ValidarSupervisorAsync(string email, string password);
}
