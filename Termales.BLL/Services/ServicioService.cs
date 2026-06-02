using Termales.BLL.Interfaces;
using Termales.Common.DTOs;
using Termales.Common.Wrappers;
using Termales.DAL.UnitOfWork;
using Termales.Entities.Models;

namespace Termales.BLL.Services;

public class ServicioService : IServicioService
{
    private readonly IUnitOfWork _uow;

    public ServicioService(IUnitOfWork uow) => _uow = uow;

    public async Task<ApiResponse<IEnumerable<ServicioDto>>> ObtenerTodosAsync()
    {
        var servicios = await _uow.Servicios.ObtenerTodosAsync();
        return ApiResponse<IEnumerable<ServicioDto>>.Exitoso(servicios.Select(MapearDto));
    }

    public async Task<ApiResponse<IEnumerable<ServicioDto>>> ObtenerActivosAsync()
    {
        var servicios = await _uow.Servicios.ObtenerActivosAsync();
        return ApiResponse<IEnumerable<ServicioDto>>.Exitoso(servicios.Select(MapearDto));
    }

    public async Task<ApiResponse<ServicioDto>> ObtenerPorIdAsync(int id)
    {
        var servicio = await _uow.Servicios.ObtenerPorIdAsync(id);
        if (servicio is null)
            return ApiResponse<ServicioDto>.Fallido("Servicio no encontrado");
        return ApiResponse<ServicioDto>.Exitoso(MapearDto(servicio));
    }

    public async Task<ApiResponse<ServicioDto>> CrearAsync(CrearServicioDto dto)
    {
        var servicio = new Servicio
        {
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            Precio = dto.Precio
        };

        await _uow.Servicios.AgregarAsync(servicio);
        await _uow.GuardarCambiosAsync();
        return ApiResponse<ServicioDto>.Exitoso(MapearDto(servicio), "Servicio registrado exitosamente");
    }

    public async Task<ApiResponse<ServicioDto>> ActualizarAsync(ActualizarServicioDto dto)
    {
        var servicio = await _uow.Servicios.ObtenerPorIdAsync(dto.ServicioId);
        if (servicio is null)
            return ApiResponse<ServicioDto>.Fallido("Servicio no encontrado");

        servicio.Nombre = dto.Nombre;
        servicio.Descripcion = dto.Descripcion;
        servicio.Precio = dto.Precio;
        servicio.Activo = dto.Activo;

        await _uow.Servicios.ActualizarAsync(servicio);
        await _uow.GuardarCambiosAsync();
        return ApiResponse<ServicioDto>.Exitoso(MapearDto(servicio), "Servicio actualizado exitosamente");
    }

    public async Task<ApiResponse> CambiarEstadoAsync(int id, bool activo)
    {
        var servicio = await _uow.Servicios.ObtenerPorIdAsync(id);
        if (servicio is null)
            return ApiResponse.Fallido("Servicio no encontrado");

        servicio.Activo = activo;
        await _uow.Servicios.ActualizarAsync(servicio);
        await _uow.GuardarCambiosAsync();
        return ApiResponse.Exitoso(activo ? "Servicio activado" : "Servicio desactivado");
    }

    private static ServicioDto MapearDto(Servicio s) => new()
    {
        ServicioId = s.ServicioId,
        Nombre = s.Nombre,
        Descripcion = s.Descripcion,
        Precio = s.Precio,
        Activo = s.Activo
    };
}
