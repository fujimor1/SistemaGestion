using Termales.BLL.Interfaces.Comedor;
using Termales.Common.DTOs.Comedor;
using Termales.Common.Wrappers;
using Termales.DAL.UnitOfWork;
using Termales.Entities.Enums;
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

    public async Task<ApiResponse> UnirAsync(int mesaPrincipalId, int mesaSecundariaId)
    {
        if (mesaPrincipalId == mesaSecundariaId)
            return ApiResponse.Fallido("Selecciona dos mesas distintas para unir");

        var principal = await _uow.Mesas.ObtenerPorIdAsync(mesaPrincipalId);
        if (principal is null || !principal.Activo)
            return ApiResponse.Fallido("Mesa principal no encontrada");

        var secundaria = await _uow.Mesas.ObtenerPorIdAsync(mesaSecundariaId);
        if (secundaria is null || !secundaria.Activo)
            return ApiResponse.Fallido("Mesa secundaria no encontrada");

        if (principal.MesaPrincipalId is not null)
            return ApiResponse.Fallido("Esa mesa ya está unida a otra — únela desde su mesa principal");
        if (secundaria.MesaPrincipalId is not null)
            return ApiResponse.Fallido("Esa mesa ya está unida a un grupo");
        if (secundaria.Estado != EstadoMesa.Disponible)
            return ApiResponse.Fallido("La mesa secundaria debe estar disponible para poder unirla");

        secundaria.MesaPrincipalId = principal.MesaId;
        secundaria.Estado = principal.Estado;
        await _uow.Mesas.ActualizarAsync(secundaria);
        await _uow.GuardarCambiosAsync();
        return ApiResponse.Exitoso($"Mesa {secundaria.Numero} unida a la mesa {principal.Numero}");
    }

    public async Task<ApiResponse> SepararAsync(int mesaId)
    {
        var mesa = await _uow.Mesas.ObtenerPorIdAsync(mesaId);
        if (mesa is null)
            return ApiResponse.Fallido("Mesa no encontrada");
        if (mesa.MesaPrincipalId is null)
            return ApiResponse.Fallido("Esta mesa no está unida a ninguna otra");

        mesa.MesaPrincipalId = null;
        mesa.Estado = EstadoMesa.Disponible;
        await _uow.Mesas.ActualizarAsync(mesa);
        await _uow.GuardarCambiosAsync();
        return ApiResponse.Exitoso($"Mesa {mesa.Numero} separada del grupo");
    }

    private static MesaDto MapearDto(Mesa m) => new()
    {
        MesaId = m.MesaId,
        Numero = m.Numero,
        Capacidad = m.Capacidad,
        Estado = m.Estado,
        Activo = m.Activo,
        MesaPrincipalId = m.MesaPrincipalId,
        NumerosMesasSecundarias = m.MesasSecundarias.Select(s => s.Numero).OrderBy(n => n).ToList(),
    };
}
