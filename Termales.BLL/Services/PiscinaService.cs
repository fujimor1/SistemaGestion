using Termales.BLL.Interfaces;
using Termales.Common.DTOs;
using Termales.Common.Wrappers;
using Termales.DAL.UnitOfWork;
using Termales.Entities.Models;

namespace Termales.BLL.Services;

public class PiscinaService : IPiscinaService
{
    private readonly IUnitOfWork _uow;

    public PiscinaService(IUnitOfWork uow) => _uow = uow;

    public async Task<ApiResponse<IEnumerable<PiscinaDto>>> ObtenerTodasAsync()
    {
        var piscinas = await _uow.Piscinas.ObtenerTodosAsync();
        return ApiResponse<IEnumerable<PiscinaDto>>.Exitoso(piscinas.Select(MapearDto));
    }

    public async Task<ApiResponse<IEnumerable<PiscinaDto>>> ObtenerDisponiblesAsync()
    {
        var piscinas = await _uow.Piscinas.ObtenerDisponiblesAsync();
        return ApiResponse<IEnumerable<PiscinaDto>>.Exitoso(piscinas.Select(MapearDto));
    }

    public async Task<ApiResponse<IEnumerable<PiscinaDto>>> ObtenerDisponiblesEnFechaAsync(DateTime ingreso, DateTime salida)
    {
        if (salida <= ingreso)
            return ApiResponse<IEnumerable<PiscinaDto>>.Fallido("La fecha de salida debe ser posterior al ingreso");

        var piscinas = await _uow.Piscinas.ObtenerDisponiblesEnFechaAsync(ingreso, salida);
        return ApiResponse<IEnumerable<PiscinaDto>>.Exitoso(piscinas.Select(MapearDto));
    }

    public async Task<ApiResponse<PiscinaDto>> ObtenerPorIdAsync(int id)
    {
        var piscina = await _uow.Piscinas.ObtenerPorIdAsync(id);
        if (piscina is null)
            return ApiResponse<PiscinaDto>.Fallido("Piscina no encontrada");
        return ApiResponse<PiscinaDto>.Exitoso(MapearDto(piscina));
    }

    public async Task<ApiResponse<PiscinaDto>> CrearAsync(CrearPiscinaDto dto)
    {
        var piscina = new Piscina
        {
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            TemperaturaGrados = dto.TemperaturaGrados,
            CapacidadPersonas = dto.CapacidadPersonas,
            TarifaPorHora = dto.TarifaPorHora
        };

        await _uow.Piscinas.AgregarAsync(piscina);
        await _uow.GuardarCambiosAsync();
        return ApiResponse<PiscinaDto>.Exitoso(MapearDto(piscina), "Piscina registrada exitosamente");
    }

    public async Task<ApiResponse<PiscinaDto>> ActualizarAsync(ActualizarPiscinaDto dto)
    {
        var piscina = await _uow.Piscinas.ObtenerPorIdAsync(dto.PiscinaId);
        if (piscina is null)
            return ApiResponse<PiscinaDto>.Fallido("Piscina no encontrada");

        piscina.Nombre = dto.Nombre;
        piscina.Descripcion = dto.Descripcion;
        piscina.TemperaturaGrados = dto.TemperaturaGrados;
        piscina.CapacidadPersonas = dto.CapacidadPersonas;
        piscina.TarifaPorHora = dto.TarifaPorHora;
        piscina.Disponible = dto.Disponible;

        await _uow.Piscinas.ActualizarAsync(piscina);
        await _uow.GuardarCambiosAsync();
        return ApiResponse<PiscinaDto>.Exitoso(MapearDto(piscina), "Piscina actualizada exitosamente");
    }

    public async Task<ApiResponse> CambiarDisponibilidadAsync(int id, bool disponible)
    {
        var piscina = await _uow.Piscinas.ObtenerPorIdAsync(id);
        if (piscina is null)
            return ApiResponse.Fallido("Piscina no encontrada");

        piscina.Disponible = disponible;
        await _uow.Piscinas.ActualizarAsync(piscina);
        await _uow.GuardarCambiosAsync();
        return ApiResponse.Exitoso(disponible ? "Piscina habilitada" : "Piscina deshabilitada");
    }

    private static PiscinaDto MapearDto(Piscina p) => new()
    {
        PiscinaId = p.PiscinaId,
        Nombre = p.Nombre,
        Descripcion = p.Descripcion,
        TemperaturaGrados = p.TemperaturaGrados,
        CapacidadPersonas = p.CapacidadPersonas,
        TarifaPorHora = p.TarifaPorHora,
        Disponible = p.Disponible
    };
}
