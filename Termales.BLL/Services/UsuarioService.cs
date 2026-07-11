using Microsoft.EntityFrameworkCore;
using Termales.BLL.Interfaces;
using Termales.Common.DTOs;
using Termales.Common.Wrappers;
using Termales.DAL.Context;
using Termales.Entities.Models.Seguridad;

namespace Termales.BLL.Services;

public class UsuarioService : IUsuarioService
{
    private readonly TermalesDbContext _context;

    public UsuarioService(TermalesDbContext context) => _context = context;

    public async Task<ApiResponse<IEnumerable<UsuarioDto>>> ObtenerTodosAsync()
    {
        var usuarios = await _context.Usuarios
            .Include(u => u.Rol)
            .Include(u => u.Empleado)
            .Where(u => u.Activo)
            .OrderBy(u => u.Empleado.Apellidos)
            .ToListAsync();

        return ApiResponse<IEnumerable<UsuarioDto>>.Exitoso(usuarios.Select(MapearDto));
    }

    public async Task<ApiResponse<UsuarioDto>> ObtenerPorIdAsync(int id)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.Rol)
            .Include(u => u.Empleado)
            .FirstOrDefaultAsync(u => u.UsuarioId == id);

        if (usuario is null)
            return ApiResponse<UsuarioDto>.Fallido("Usuario no encontrado");

        return ApiResponse<UsuarioDto>.Exitoso(MapearDto(usuario));
    }

    public async Task<ApiResponse<UsuarioDto>> CrearAsync(CrearUsuarioDto dto)
    {
        if (await _context.Usuarios.AnyAsync(u => u.Email == dto.Email))
            return ApiResponse<UsuarioDto>.Fallido($"El email {dto.Email} ya está en uso");

        var rolExiste = await _context.Roles.AnyAsync(r => r.RolId == dto.RolId && r.Activo);
        if (!rolExiste)
            return ApiResponse<UsuarioDto>.Fallido("El rol especificado no existe");

        var empleadoExiste = await _context.Empleados.AnyAsync(e => e.EmpleadoId == dto.EmpleadoId);
        if (!empleadoExiste)
            return ApiResponse<UsuarioDto>.Fallido("El empleado especificado no existe");

        if (await _context.Usuarios.AnyAsync(u => u.EmpleadoId == dto.EmpleadoId))
            return ApiResponse<UsuarioDto>.Fallido("Ese empleado ya tiene una cuenta de usuario vinculada");

        var usuario = new Usuario
        {
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            RolId = dto.RolId,
            EmpleadoId = dto.EmpleadoId,
            Activo = true
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        await _context.Entry(usuario).Reference(u => u.Rol).LoadAsync();
        await _context.Entry(usuario).Reference(u => u.Empleado).LoadAsync();
        return ApiResponse<UsuarioDto>.Exitoso(MapearDto(usuario), "Usuario creado exitosamente");
    }

    public async Task<ApiResponse<UsuarioDto>> ActualizarAsync(ActualizarUsuarioDto dto)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.Rol)
            .Include(u => u.Empleado)
            .FirstOrDefaultAsync(u => u.UsuarioId == dto.UsuarioId);

        if (usuario is null)
            return ApiResponse<UsuarioDto>.Fallido("Usuario no encontrado");

        if (await _context.Usuarios.AnyAsync(u => u.Email == dto.Email && u.UsuarioId != dto.UsuarioId))
            return ApiResponse<UsuarioDto>.Fallido($"El email {dto.Email} ya está en uso por otro usuario");

        var rolExiste = await _context.Roles.AnyAsync(r => r.RolId == dto.RolId && r.Activo);
        if (!rolExiste)
            return ApiResponse<UsuarioDto>.Fallido("El rol especificado no existe");

        var empleadoExisteAct = await _context.Empleados.AnyAsync(e => e.EmpleadoId == dto.EmpleadoId);
        if (!empleadoExisteAct)
            return ApiResponse<UsuarioDto>.Fallido("El empleado especificado no existe");

        if (await _context.Usuarios.AnyAsync(u => u.EmpleadoId == dto.EmpleadoId && u.UsuarioId != dto.UsuarioId))
            return ApiResponse<UsuarioDto>.Fallido("Ese empleado ya tiene una cuenta de usuario vinculada");

        usuario.Email = dto.Email;
        usuario.RolId = dto.RolId;
        usuario.EmpleadoId = dto.EmpleadoId;

        await _context.SaveChangesAsync();
        await _context.Entry(usuario).Reference(u => u.Rol).LoadAsync();
        await _context.Entry(usuario).Reference(u => u.Empleado).LoadAsync();
        return ApiResponse<UsuarioDto>.Exitoso(MapearDto(usuario), "Usuario actualizado exitosamente");
    }

    public async Task<ApiResponse<IEnumerable<RolDto>>> ObtenerRolesAsync()
    {
        var roles = await _context.Roles
            .Where(r => r.Activo)
            .OrderBy(r => r.Nombre)
            .Select(r => new RolDto { RolId = r.RolId, Nombre = r.Nombre, Descripcion = r.Descripcion })
            .ToListAsync();

        return ApiResponse<IEnumerable<RolDto>>.Exitoso(roles);
    }

    public async Task<ApiResponse> CambiarPasswordAsync(int id, CambiarPasswordDto dto)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario is null)
            return ApiResponse.Fallido("Usuario no encontrado");

        usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NuevaPassword);
        await _context.SaveChangesAsync();
        return ApiResponse.Exitoso("Contraseña actualizada exitosamente");
    }

    public async Task<ApiResponse> DesactivarAsync(int id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario is null)
            return ApiResponse.Fallido("Usuario no encontrado");

        usuario.Activo = false;
        await _context.SaveChangesAsync();
        return ApiResponse.Exitoso("Usuario desactivado exitosamente");
    }

    private static UsuarioDto MapearDto(Usuario u) => new()
    {
        UsuarioId = u.UsuarioId,
        Email = u.Email,
        RolId = u.RolId,
        NombreRol = u.Rol?.Nombre ?? string.Empty,
        Activo = u.Activo,
        FechaCreacion = u.FechaCreacion,
        EmpleadoId = u.EmpleadoId,
        NombreEmpleado = $"{u.Empleado.Nombres} {u.Empleado.Apellidos}"
    };
}
