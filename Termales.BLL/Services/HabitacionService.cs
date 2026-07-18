using Termales.BLL.Interfaces;
using Termales.Common.DTOs;
using Termales.Common.Wrappers;
using Termales.DAL.UnitOfWork;
using Termales.Entities.Enums;
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
            EstadoLimpieza = EstadoLimpieza.Limpia,
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
            // Al liberar, la habitación queda pendiente de limpieza — el personal de
            // limpieza la marca lista antes de poder asignarla a un cliente nuevo.
            h.EstadoLimpieza = EstadoLimpieza.PorLimpiar;
        }
        await _uow.Habitaciones.ActualizarAsync(h);
        await _uow.GuardarCambiosAsync();
        return ApiResponse.Exitoso(ocupado ? "Habitación marcada como ocupada" : "Habitación liberada, pendiente de limpieza");
    }

    public async Task<ApiResponse> MarcarLimpiaAsync(int id)
    {
        var h = await _uow.Habitaciones.ObtenerPorIdAsync(id);
        if (h is null)
            return ApiResponse.Fallido("Habitación no encontrada");
        if (h.Ocupado)
            return ApiResponse.Fallido("La habitación está ocupada, no se puede marcar limpia todavía");

        h.EstadoLimpieza = EstadoLimpieza.Limpia;
        await _uow.Habitaciones.ActualizarAsync(h);
        await _uow.GuardarCambiosAsync();
        return ApiResponse.Exitoso("Habitación marcada como limpia — ya se puede asignar");
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

    public async Task<ApiResponse<IEnumerable<HabitacionItemDto>>> ObtenerItemsAsync(int habitacionId)
    {
        var items = await _uow.HabitacionItems.ObtenerPorHabitacionAsync(habitacionId);
        return ApiResponse<IEnumerable<HabitacionItemDto>>.Exitoso(items.Select(MapearItemDto));
    }

    public async Task<ApiResponse<HabitacionItemDto>> AgregarItemAsync(int habitacionId, CrearHabitacionItemDto dto)
    {
        var h = await _uow.Habitaciones.ObtenerPorIdAsync(habitacionId);
        if (h is null)
            return ApiResponse<HabitacionItemDto>.Fallido("Habitación no encontrada");

        var item = new HabitacionItem
        {
            HabitacionId = habitacionId,
            Nombre = dto.Nombre,
            Cantidad = dto.Cantidad,
        };
        await _uow.HabitacionItems.AgregarAsync(item);
        await _uow.GuardarCambiosAsync();
        return ApiResponse<HabitacionItemDto>.Exitoso(MapearItemDto(item), "Ítem agregado");
    }

    public async Task<ApiResponse> EliminarItemAsync(int habitacionItemId)
    {
        var item = await _uow.HabitacionItems.ObtenerPorIdAsync(habitacionItemId);
        if (item is null)
            return ApiResponse.Fallido("Ítem no encontrado");

        await _uow.HabitacionItems.EliminarAsync(habitacionItemId);
        await _uow.GuardarCambiosAsync();
        return ApiResponse.Exitoso("Ítem eliminado");
    }

    private static HabitacionItemDto MapearItemDto(HabitacionItem i) => new()
    {
        HabitacionItemId = i.HabitacionItemId,
        HabitacionId = i.HabitacionId,
        Nombre = i.Nombre,
        Cantidad = i.Cantidad,
    };

    private static HabitacionDto MapearDto(Habitacion h) => new()
    {
        HabitacionId = h.HabitacionId,
        Nombre = h.Nombre,
        Descripcion = h.Descripcion,
        Capacidad = h.Capacidad,
        Precio = h.Precio,
        Ocupado = h.Ocupado,
        Activo = h.Activo,
        EstadoLimpieza = (int)h.EstadoLimpieza,
        EstadoLimpiezaDescripcion = h.EstadoLimpieza == EstadoLimpieza.PorLimpiar ? "Por limpiar" : "Limpia",
        FechaCheckIn = h.FechaCheckIn,
        FechaCheckOut = h.FechaCheckOut
    };
}
