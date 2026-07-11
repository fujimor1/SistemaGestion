using Termales.BLL.Interfaces.Inventario;
using Termales.Common.DTOs.Inventario;
using Termales.DAL.UnitOfWork;
using Termales.Entities.Models.Inventario;

namespace Termales.BLL.Services.Inventario;

public class InsumoService : IInsumoService
{
    private readonly IUnitOfWork _uow;

    public InsumoService(IUnitOfWork uow) => _uow = uow;

    public async Task<IEnumerable<InsumoDto>> ObtenerPorAmbienteAsync(string tipoAmbiente)
    {
        var insumos = await _uow.Insumos.ObtenerPorAmbienteAsync(tipoAmbiente);
        return insumos.Select(MapDto);
    }

    public async Task<InsumoDto?> ObtenerPorIdAsync(int id)
    {
        var insumo = await _uow.Insumos.ObtenerPorIdAsync(id);
        return insumo is null ? null : MapDto(insumo);
    }

    public async Task<InsumoDto> CrearAsync(CrearInsumoDto dto)
    {
        var insumo = new Insumo
        {
            Nombre = dto.Nombre,
            TipoAmbiente = dto.TipoAmbiente,
            TipoArticulo = dto.TipoArticulo,
            Unidad = dto.Unidad,
            StockActual = dto.StockActual,
            PrecioReferencia = dto.PrecioReferencia
        };
        await _uow.Insumos.AgregarAsync(insumo);
        await _uow.GuardarCambiosAsync();
        return MapDto(insumo);
    }

    public async Task<InsumoDto?> ActualizarAsync(int id, ActualizarInsumoDto dto)
    {
        var insumo = await _uow.Insumos.ObtenerPorIdAsync(id);
        if (insumo is null) return null;

        insumo.Nombre = dto.Nombre;
        insumo.Unidad = dto.Unidad;
        insumo.PrecioReferencia = dto.PrecioReferencia;
        insumo.Activo = dto.Activo;

        await _uow.Insumos.ActualizarAsync(insumo);
        await _uow.GuardarCambiosAsync();
        return MapDto(insumo);
    }

    public async Task<bool> EliminarAsync(int id)
    {
        var insumo = await _uow.Insumos.ObtenerPorIdAsync(id);
        if (insumo is null) return false;

        insumo.Activo = false;
        await _uow.Insumos.ActualizarAsync(insumo);
        await _uow.GuardarCambiosAsync();
        return true;
    }

    private static InsumoDto MapDto(Insumo i) => new()
    {
        InsumoId = i.InsumoId,
        Nombre = i.Nombre,
        TipoAmbiente = i.TipoAmbiente,
        TipoArticulo = i.TipoArticulo,
        Unidad = i.Unidad,
        StockActual = i.StockActual,
        PrecioReferencia = i.PrecioReferencia,
        Activo = i.Activo,
        FechaRegistro = i.FechaRegistro
    };
}
