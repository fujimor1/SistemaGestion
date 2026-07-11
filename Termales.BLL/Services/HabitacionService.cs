using Termales.BLL.Interfaces;
using Termales.Common.DTOs;
using Termales.Common.Wrappers;
using Termales.DAL.UnitOfWork;
using Termales.Entities.Models;

namespace Termales.BLL.Services;

public class HabitacionService : IHabitacionService
{
    private readonly IUnitOfWork _uow;

    public HabitacionService(IUnitOfWork uow) => _uow = uow;

    public async Task<ApiResponse<IEnumerable<HabitacionDto>>> ObtenerTodasAsync()
    {
        var habitaciones = await _uow.Habitaciones.ObtenerActivasAsync();
        return ApiResponse<IEnumerable<HabitacionDto>>.Exitoso(habitaciones.Select(MapearDto));
    }

    public async Task<ApiResponse<HabitacionDto>> ObtenerPorIdAsync(int id)
    {
        var h = await _uow.Habitaciones.ObtenerPorIdAsync(id);
        if (h is null)
            return ApiResponse<HabitacionDto>.Fallido("Habitación no encontrada");
        return ApiResponse<HabitacionDto>.Exitoso(MapearDto(h));
    }

    public async Task<ApiResponse<HabitacionDto>> CrearAsync(CrearHabitacionDto dto)
    {
        var habitacion = new Habitacion
        {
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            Capacidad = dto.Capacidad,
            Precio = dto.Precio,
            Ocupado = false,
            Activo = true
        };

        await _uow.Habitaciones.AgregarAsync(habitacion);
        await _uow.GuardarCambiosAsync();
        return ApiResponse<HabitacionDto>.Exitoso(MapearDto(habitacion), "Habitación creada exitosamente");
    }

    public async Task<ApiResponse<HabitacionDto>> ActualizarAsync(ActualizarHabitacionDto dto)
    {
        var h = await _uow.Habitaciones.ObtenerPorIdAsync(dto.HabitacionId);
        if (h is null)
            return ApiResponse<HabitacionDto>.Fallido("Habitación no encontrada");

        h.Nombre = dto.Nombre;
        h.Descripcion = dto.Descripcion;
        h.Capacidad = dto.Capacidad;
        h.Precio = dto.Precio;

        await _uow.Habitaciones.ActualizarAsync(h);
        await _uow.GuardarCambiosAsync();
        return ApiResponse<HabitacionDto>.Exitoso(MapearDto(h), "Habitación actualizada exitosamente");
    }

    public async Task<ApiResponse> CambiarOcupacionAsync(int id, bool ocupado)
    {
        var h = await _uow.Habitaciones.ObtenerPorIdAsync(id);
        if (h is null)
            return ApiResponse.Fallido("Habitación no encontrada");

        h.Ocupado = ocupado;
        if (ocupado)
        {
            h.FechaCheckIn = DateTime.UtcNow;
            h.FechaCheckOut = null;
        }
        else
        {
            h.FechaCheckOut = DateTime.UtcNow;
        }
        await _uow.Habitaciones.ActualizarAsync(h);
        await _uow.GuardarCambiosAsync();
        return ApiResponse.Exitoso(ocupado ? "Habitación marcada como ocupada" : "Habitación marcada como libre");
    }

    public async Task<ApiResponse> EliminarAsync(int id)
    {
        var h = await _uow.Habitaciones.ObtenerPorIdAsync(id);
        if (h is null)
            return ApiResponse.Fallido("Habitación no encontrada");

        h.Activo = false;
        await _uow.Habitaciones.ActualizarAsync(h);
        await _uow.GuardarCambiosAsync();
        return ApiResponse.Exitoso("Habitación eliminada exitosamente");
    }

    private static HabitacionDto MapearDto(Habitacion h) => new()
    {
        HabitacionId = h.HabitacionId,
        Nombre = h.Nombre,
        Descripcion = h.Descripcion,
        Capacidad = h.Capacidad,
        Precio = h.Precio,
        Ocupado = h.Ocupado,
        Activo = h.Activo,
        FechaCheckIn = h.FechaCheckIn,
        FechaCheckOut = h.FechaCheckOut
    };
}
