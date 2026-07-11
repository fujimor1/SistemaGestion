using Termales.BLL.Interfaces;
using Termales.Common.DTOs;
using Termales.Common.Wrappers;
using Termales.DAL.UnitOfWork;
using Termales.Entities.Models;

namespace Termales.BLL.Services;

public class TipoServicioService : ITipoServicioService
{
    private readonly IUnitOfWork _uow;

    public TipoServicioService(IUnitOfWork uow) => _uow = uow;

    public async Task<ApiResponse<TipoServicioDto>> ObtenerPorIdAsync(int id)
    {
        var ts = await _uow.TiposServicio.ObtenerPorIdAsync(id);
        if (ts is null)
            return ApiResponse<TipoServicioDto>.Fallido("Tipo de servicio no encontrado");
        return ApiResponse<TipoServicioDto>.Exitoso(MapearDto(ts));
    }

    public async Task<ApiResponse<IEnumerable<TipoServicioDto>>> ObtenerTodosAsync()
    {
        var todos = await _uow.TiposServicio.ObtenerTodosAsync();
        return ApiResponse<IEnumerable<TipoServicioDto>>.Exitoso(todos.Select(MapearDto));
    }

    public async Task<ApiResponse<IEnumerable<TipoServicioDto>>> ObtenerActivosAsync()
    {
        var activos = await _uow.TiposServicio.ObtenerActivosAsync();
        return ApiResponse<IEnumerable<TipoServicioDto>>.Exitoso(activos.Select(MapearDto));
    }

    public async Task<ApiResponse<TipoServicioDto>> CrearAsync(CrearTipoServicioDto dto)
    {
        if (await _uow.TiposServicio.ExisteAsync(t => t.Nombre == dto.Nombre))
            return ApiResponse<TipoServicioDto>.Fallido($"Ya existe un tipo de servicio con el nombre '{dto.Nombre}'");

        var tipoServicio = new TipoServicio
        {
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            CapacidadMaxima = dto.CapacidadMaxima,
            PrecioPorPersona = dto.PrecioPorPersona,
            Activo = true
        };

        await _uow.TiposServicio.AgregarAsync(tipoServicio);
        await _uow.GuardarCambiosAsync();
        return ApiResponse<TipoServicioDto>.Exitoso(MapearDto(tipoServicio), "Tipo de servicio creado exitosamente");
    }

    public async Task<ApiResponse<TipoServicioDto>> ActualizarAsync(ActualizarTipoServicioDto dto)
    {
        var ts = await _uow.TiposServicio.ObtenerPorIdAsync(dto.TipoServicioId);
        if (ts is null)
            return ApiResponse<TipoServicioDto>.Fallido("Tipo de servicio no encontrado");

        if (await _uow.TiposServicio.ExisteAsync(t => t.Nombre == dto.Nombre && t.TipoServicioId != dto.TipoServicioId))
            return ApiResponse<TipoServicioDto>.Fallido($"El nombre '{dto.Nombre}' ya pertenece a otro tipo de servicio");

        ts.Nombre = dto.Nombre;
        ts.Descripcion = dto.Descripcion;
        ts.CapacidadMaxima = dto.CapacidadMaxima;
        ts.PrecioPorPersona = dto.PrecioPorPersona;

        await _uow.TiposServicio.ActualizarAsync(ts);
        await _uow.GuardarCambiosAsync();
        return ApiResponse<TipoServicioDto>.Exitoso(MapearDto(ts), "Tipo de servicio actualizado exitosamente");
    }

    public async Task<ApiResponse> DesactivarAsync(int id)
    {
        var ts = await _uow.TiposServicio.ObtenerPorIdAsync(id);
        if (ts is null)
            return ApiResponse.Fallido("Tipo de servicio no encontrado");

        ts.Activo = false;
        await _uow.TiposServicio.ActualizarAsync(ts);
        await _uow.GuardarCambiosAsync();
        return ApiResponse.Exitoso("Tipo de servicio desactivado exitosamente");
    }

    private static TipoServicioDto MapearDto(TipoServicio t) => new()
    {
        TipoServicioId = t.TipoServicioId,
        Nombre = t.Nombre,
        Descripcion = t.Descripcion,
        CapacidadMaxima = t.CapacidadMaxima,
        PrecioPorPersona = t.PrecioPorPersona,
        Activo = t.Activo
    };
}
