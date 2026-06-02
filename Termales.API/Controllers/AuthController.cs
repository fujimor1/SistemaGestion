using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Termales.BLL.Interfaces;
using Termales.Common.DTOs.Auth;

namespace Termales.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    /// <summary>
    /// Autentica al usuario y devuelve un JWT de 24 horas.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var resultado = await _authService.LoginAsync(dto);
        return resultado.Exito ? Ok(resultado) : Unauthorized(resultado);
    }

    /// <summary>
    /// Invalida el JWT actual del usuario autenticado.
    /// </summary>
    [Authorize]
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        var jti = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value ?? string.Empty;
        var expClaim = User.FindFirst(JwtRegisteredClaimNames.Exp)?.Value;

        DateTime expiracion = DateTime.UtcNow;
        if (long.TryParse(expClaim, out var expUnix))
            expiracion = DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;

        var resultado = _authService.LogoutAsync(jti, expiracion);
        return resultado.Exito ? Ok(resultado) : BadRequest(resultado);
    }
}
