using Termales.BLL.Interfaces;
using Termales.Common.DTOs;
using Termales.Common.Wrappers;
using Termales.DAL.UnitOfWork;
using Termales.Entities.Models;

namespace Termales.BLL.Services;

public class AforoService : IAforoService
{
    private readonly IUnitOfWork _uow;

    public AforoService(IUnitOfWork uow) => _uow = uow;

    public async Task<ApiResponse<AforoDto>> ObtenerPorIdAsync(int id)
    {
        var aforo = await _uow.Aforos.ObtenerPorIdAsync(id);
        if (aforo is null)
            return ApiResponse<AforoDto>.Fallido("Aforo no encontrado");

        var ts = await _uow.TiposServicio.ObtenerPorIdAsync(aforo.TipoServicioId);
        return ApiResponse<AforoDto>.Exitoso(MapearDto(aforo, ts?.Nombre));
    }

    public async Task<ApiResponse<IEnumerable<AforoDto>>> ObtenerPorFechaAsync(DateTime fecha)
    {
        var aforos = await _uow.Aforos.ObtenerPorFechaAsync(fecha);
        return ApiResponse<IEnumerable<AforoDto>>.Exitoso(
            aforos.Select(a => MapearDto(a, a.TipoServicio?.Nombre)));
    }

    public async Task<ApiResponse<AforoDto>> ObtenerPorTipoYFechaAsync(int tipoServicioId, DateTime fecha)
    {
        var aforo = await _uow.Aforos.ObtenerPorTipoYFechaAsync(tipoServicioId, fecha);
        if (aforo is null)
            return ApiResponse<AforoDto>.Fallido("No se encontró aforo para esa fecha y tipo de servicio");
        return ApiResponse<AforoDto>.Exitoso(MapearDto(aforo, aforo.TipoServicio?.Nombre));
    }

    public async Task<ApiResponse<AforoDto>> CrearAsync(CrearAforoDto dto)
    {
        var ts = await _uow.TiposServicio.ObtenerPorIdAsync(dto.TipoServicioId);
        if (ts is null)
            return ApiResponse<AforoDto>.Fallido("Tipo de servicio no encontrado");

        var existente = await _uow.Aforos.ObtenerPorTipoYFechaAsync(dto.TipoServicioId, dto.Fecha);
        if (existente is not null)
            return ApiResponse<AforoDto>.Fallido("Ya existe un aforo para esa fecha y tipo de servicio");

        var aforo = new Aforo
        {
            TipoServicioId = dto.TipoServicioId,
            Fecha = dto.Fecha.Date,
            CapacidadMaxima = dto.CapacidadMaxima,
            OcupacionActual = 0
        };

        await _uow.Aforos.AgregarAsync(aforo);
        await _uow.GuardarCambiosAsync();
        return ApiResponse<AforoDto>.Exitoso(MapearDto(aforo, ts.Nombre), "Aforo creado exitosamente");
    }

    public async Task<ApiResponse<AforoDto>> ActualizarAsync(ActualizarAforoDto dto)
    {
        var aforo = await _uow.Aforos.ObtenerPorIdAsync(dto.AforoId);
        if (aforo is null)
            return ApiResponse<AforoDto>.Fallido("Aforo no encontrado");

        aforo.CapacidadMaxima = dto.CapacidadMaxima;

        await _uow.Aforos.ActualizarAsync(aforo);
        await _uow.GuardarCambiosAsync();

        var ts = await _uow.TiposServicio.ObtenerPorIdAsync(aforo.TipoServicioId);
        return ApiResponse<AforoDto>.Exitoso(MapearDto(aforo, ts?.Nombre), "Aforo actualizado exitosamente");
    }

    private static AforoDto MapearDto(Aforo a, string? nombreTipoServicio) => new()
    {
        AforoId = a.AforoId,
        TipoServicioId = a.TipoServicioId,
        NombreTipoServicio = nombreTipoServicio ?? string.Empty,
        Fecha = a.Fecha,
        CapacidadMaxima = a.CapacidadMaxima,
        OcupacionActual = a.OcupacionActual
    };
}
