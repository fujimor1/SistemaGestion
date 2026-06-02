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

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var resultado = await _authService.LoginAsync(dto);
        return resultado.Exito ? Ok(resultado) : Unauthorized(resultado);
    }
}
