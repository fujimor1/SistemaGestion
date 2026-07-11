using Termales.BLL.Interfaces.Comedor;
using Termales.Common.DTOs.Comedor;
using Termales.Common.Wrappers;
using Termales.DAL.UnitOfWork;
using Termales.Entities.Models.Comedor;

namespace Termales.BLL.Services.Comedor;

public class MesaService : IMesaService
{
    private readonly IUnitOfWork _uow;

    public MesaService(IUnitOfWork uow) => _uow = uow;

    public async Task<ApiResponse<IEnumerable<MesaDto>>> ObtenerTodasAsync()
    {
        var mesas = await _uow.Mesas.ObtenerActivasAsync();
        return ApiResponse<IEnumerable<MesaDto>>.Exitoso(mesas.Select(MapearDto));
    }

    public async Task<ApiResponse<MesaDto>> ObtenerPorIdAsync(int id)
    {
        var mesa = await _uow.Mesas.ObtenerPorIdAsync(id);
        if (mesa is null)
            return ApiResponse<MesaDto>.Fallido("Mesa no encontrada");
        return ApiResponse<MesaDto>.Exitoso(MapearDto(mesa));
    }

    public async Task<ApiResponse<MesaDto>> CrearAsync(CrearMesaDto dto)
    {
        if (await _uow.Mesas.ExisteAsync(m => m.Numero == dto.Numero))
            return ApiResponse<MesaDto>.Fallido($"Ya existe la mesa número {dto.Numero}");

        var mesa = new Mesa { Numero = dto.Numero, Capacidad = dto.Capacidad };
        await _uow.Mesas.AgregarAsync(mesa);
        await _uow.GuardarCambiosAsync();
        return ApiResponse<MesaDto>.Exitoso(MapearDto(mesa), "Mesa creada exitosamente");
    }

    public async Task<ApiResponse<MesaDto>> ActualizarAsync(ActualizarMesaDto dto)
    {
        var mesa = await _uow.Mesas.ObtenerPorIdAsync(dto.MesaId);
        if (mesa is null)
            return ApiResponse<MesaDto>.Fallido("Mesa no encontrada");

        if (await _uow.Mesas.ExisteAsync(m => m.Numero == dto.Numero && m.MesaId != dto.MesaId))
            return ApiResponse<MesaDto>.Fallido($"El número {dto.Numero} ya está en uso");

        mesa.Numero = dto.Numero;
        mesa.Capacidad = dto.Capacidad;
        await _uow.Mesas.ActualizarAsync(mesa);
        await _uow.GuardarCambiosAsync();
        return ApiResponse<MesaDto>.Exitoso(MapearDto(mesa), "Mesa actualizada exitosamente");
    }

    public async Task<ApiResponse> DesactivarAsync(int id)
    {
        var mesa = await _uow.Mesas.ObtenerPorIdAsync(id);
        if (mesa is null)
            return ApiResponse.Fallido("Mesa no encontrada");

        mesa.Activo = false;
        await _uow.Mesas.ActualizarAsync(mesa);
        await _uow.GuardarCambiosAsync();
        return ApiResponse.Exitoso("Mesa desactivada exitosamente");
    }

    private static MesaDto MapearDto(Mesa m) => new()
    {
        MesaId = m.MesaId,
        Numero = m.Numero,
        Capacidad = m.Capacidad,
        Estado = m.Estado,
        Activo = m.Activo
    };
}
