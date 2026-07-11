using Microsoft.EntityFrameworkCore;
using Termales.BLL.Interfaces;
using Termales.Common.DTOs;
using Termales.Common.Wrappers;
using Termales.DAL.Context;
using Termales.DAL.UnitOfWork;
using Termales.Entities.Models;

namespace Termales.BLL.Services;

public class EmpleadoService : IEmpleadoService
{
    private readonly IUnitOfWork _uow;
    private readonly TermalesDbContext _context;

    public EmpleadoService(IUnitOfWork uow, TermalesDbContext context)
    {
        _uow = uow;
        _context = context;
    }

    private async Task<HashSet<int>> ObtenerEmpleadosConUsuarioAsync(params int[] empleadoIds) =>
        (await _context.Usuarios
            .Where(u => empleadoIds.Contains(u.EmpleadoId))
            .Select(u => u.EmpleadoId)
            .ToListAsync())
        .ToHashSet();

    public async Task<ApiResponse<EmpleadoDto>> ObtenerPorIdAsync(int id)
    {
        var empleado = await _uow.Empleados.ObtenerPorIdAsync(id);
        if (empleado is null)
            return ApiResponse<EmpleadoDto>.Fallido("Empleado no encontrado");
        var conUsuario = await ObtenerEmpleadosConUsuarioAsync(id);
        return ApiResponse<EmpleadoDto>.Exitoso(MapearDto(empleado, conUsuario.Contains(id)));
    }

    public async Task<ApiResponse<EmpleadoDto>> ObtenerPorDniAsync(string dni)
    {
        var empleado = await _uow.Empleados.ObtenerPorDniAsync(dni);
        if (empleado is null)
            return ApiResponse<EmpleadoDto>.Fallido("Empleado no encontrado");
        var conUsuario = await ObtenerEmpleadosConUsuarioAsync(empleado.EmpleadoId);
        return ApiResponse<EmpleadoDto>.Exitoso(MapearDto(empleado, conUsuario.Contains(empleado.EmpleadoId)));
    }

    public async Task<PagedResponse<EmpleadoDto>> ObtenerPaginadoAsync(int pagina, int tamanoPagina, string? busqueda)
    {
        var (items, total) = await _uow.Empleados.ObtenerPaginadoAsync(pagina, tamanoPagina, busqueda);
        var lista = items.ToList();
        var conUsuario = await ObtenerEmpleadosConUsuarioAsync(lista.Select(e => e.EmpleadoId).ToArray());
        return PagedResponse<EmpleadoDto>.Crear(lista.Select(e => MapearDto(e, conUsuario.Contains(e.EmpleadoId))), pagina, tamanoPagina, total);
    }

    public async Task<ApiResponse<EmpleadoDto>> CrearAsync(CrearEmpleadoDto dto)
    {
        if (await _uow.Empleados.ExisteAsync(e => e.Dni == dto.Dni))
            return ApiResponse<EmpleadoDto>.Fallido($"Ya existe un empleado con DNI {dto.Dni}");

        var empleado = new Empleado
        {
            Nombres = dto.Nombres,
            Apellidos = dto.Apellidos,
            Dni = dto.Dni,
            Activo = true
        };

        await _uow.Empleados.AgregarAsync(empleado);
        await _uow.GuardarCambiosAsync();
        return ApiResponse<EmpleadoDto>.Exitoso(MapearDto(empleado, tieneUsuario: false), "Empleado registrado exitosamente");
    }

    public async Task<ApiResponse<EmpleadoDto>> ActualizarAsync(ActualizarEmpleadoDto dto)
    {
        var empleado = await _uow.Empleados.ObtenerPorIdAsync(dto.EmpleadoId);
        if (empleado is null)
            return ApiResponse<EmpleadoDto>.Fallido("Empleado no encontrado");

        if (await _uow.Empleados.ExisteAsync(e => e.Dni == dto.Dni && e.EmpleadoId != dto.EmpleadoId))
            return ApiResponse<EmpleadoDto>.Fallido($"El DNI {dto.Dni} ya pertenece a otro empleado");

        empleado.Nombres = dto.Nombres;
        empleado.Apellidos = dto.Apellidos;
        empleado.Dni = dto.Dni;

        await _uow.Empleados.ActualizarAsync(empleado);
        await _uow.GuardarCambiosAsync();
        var conUsuario = await ObtenerEmpleadosConUsuarioAsync(empleado.EmpleadoId);
        return ApiResponse<EmpleadoDto>.Exitoso(MapearDto(empleado, conUsuario.Contains(empleado.EmpleadoId)), "Empleado actualizado exitosamente");
    }

    public async Task<ApiResponse> DesactivarAsync(int id)
    {
        var empleado = await _uow.Empleados.ObtenerPorIdAsync(id);
        if (empleado is null)
            return ApiResponse.Fallido("Empleado no encontrado");

        empleado.Activo = false;
        await _uow.Empleados.ActualizarAsync(empleado);
        await _uow.GuardarCambiosAsync();
        return ApiResponse.Exitoso("Empleado desactivado exitosamente");
    }

    private static EmpleadoDto MapearDto(Empleado e, bool tieneUsuario) => new()
    {
        EmpleadoId = e.EmpleadoId,
        Nombres = e.Nombres,
        Apellidos = e.Apellidos,
        Dni = e.Dni,
        Activo = e.Activo,
        TieneUsuario = tieneUsuario
    };
}
