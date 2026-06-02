using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Termales.BLL.Interfaces;
using Termales.Common.DTOs.Auth;
using Termales.Common.Settings;
using Termales.Common.Wrappers;
using Termales.DAL.Context;

namespace Termales.BLL.Services;

public class AuthService : IAuthService
{
    private readonly TermalesDbContext _context;
    private readonly JwtSettings _jwt;

    public AuthService(TermalesDbContext context, IOptions<JwtSettings> jwt)
    {
        _context = context;
        _jwt = jwt.Value;
    }

    public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto dto)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Email == dto.Email && u.Activo);

        if (usuario is null || !BCrypt.Net.BCrypt.Verify(dto.Password, usuario.PasswordHash))
            return ApiResponse<AuthResponseDto>.Fallido("Credenciales incorrectas");

        var expiracion = DateTime.UtcNow.AddHours(_jwt.DuracionHoras);
        var token = GenerarToken(usuario.UsuarioId, usuario.Email, usuario.Rol.Nombre, expiracion);

        var respuesta = new AuthResponseDto
        {
            Token = token,
            Expiracion = expiracion,
            Usuario = new UsuarioInfoDto
            {
                UsuarioId = usuario.UsuarioId,
                NombreCompleto = $"{usuario.Nombre} {usuario.Apellido}",
                Email = usuario.Email,
                Rol = usuario.Rol.Nombre
            }
        };

        return ApiResponse<AuthResponseDto>.Exitoso(respuesta, "Login exitoso");
    }

    private string GenerarToken(int usuarioId, string email, string rol, DateTime expiracion)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuarioId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(ClaimTypes.Role, rol),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var tokenDescriptor = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: expiracion,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }
}
