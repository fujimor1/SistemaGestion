using Termales.Common.DTOs.Auth;
using Termales.Common.Wrappers;

namespace Termales.BLL.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto dto);
    ApiResponse LogoutAsync(string jti, DateTime expiracionToken);
}
