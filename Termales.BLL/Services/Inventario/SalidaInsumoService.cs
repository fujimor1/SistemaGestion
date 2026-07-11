using Termales.BLL.Interfaces.Inventario;
using Termales.Common.DTOs.Inventario;
using Termales.DAL.UnitOfWork;
using Termales.Entities.Models.Inventario;

namespace Termales.BLL.Services.Inventario;

public class SalidaInsumoService : ISalidaInsumoService
{
    private readonly IUnitOfWork _uow;

    public SalidaInsumoService(IUnitOfWork uow) => _uow = uow;

    public async Task<IEnumerable<SalidaInsumoDto>> ObtenerPorInsumoAsync(int insumoId)
    {
        var salidas = await _uow.SalidasInsumo.ObtenerPorInsumoAsync(insumoId);
        return salidas.Select(s => MapDto(s));
    }

    public async Task<IEnumerable<SalidaInsumoDto>> ObtenerPorFechaAsync(DateTime fecha)
    {
        var salidas = await _uow.SalidasInsumo.ObtenerPorFechaAsync(fecha);
        return salidas.Select(s => MapDto(s));
    }

    public async Task<SalidaInsumoDto> RegistrarAsync(RegistrarSalidaInsumoDto dto)
    {
        var insumo = await _uow.Insumos.ObtenerPorIdAsync(dto.InsumoId)
            ?? throw new Exception($"Insumo {dto.InsumoId} no encontrado");

        if (insumo.StockActual < dto.Cantidad)
            throw new InvalidOperationException(
                $"Stock insuficiente. Disponible: {insumo.StockActual} {insumo.Unidad}");

        var salida = new SalidaInsumo
        {
            InsumoId = dto.InsumoId,
            Cantidad = dto.Cantidad,
            Observacion = dto.Observacion
        };

        insumo.StockActual -= dto.Cantidad;

        await _uow.SalidasInsumo.AgregarAsync(salida);
        await _uow.Insumos.ActualizarAsync(insumo);
        await _uow.GuardarCambiosAsync();

        return MapDto(salida, insumo.Nombre, insumo.Unidad);
    }

    private static SalidaInsumoDto MapDto(SalidaInsumo s, string? nombre = null, string? unidad = null) => new()
    {
        SalidaInsumoId = s.SalidaInsumoId,
        InsumoId = s.InsumoId,
        NombreInsumo = nombre ?? s.Insumo?.Nombre ?? string.Empty,
        Unidad = unidad ?? s.Insumo?.Unidad,
        Cantidad = s.Cantidad,
        Fecha = s.Fecha,
        Observacion = s.Observacion
    };
}
