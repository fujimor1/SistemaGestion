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

    public async Task<IEnumerable<SalidaInsumoDto>> ObtenerHistorialAsync(
        DateTime desde, DateTime hasta, int? insumoId, string? motivo)
    {
        var salidas = await _uow.SalidasInsumo.ObtenerHistorialAsync(desde, hasta, insumoId, motivo);
        return salidas.Select(s => MapDto(s));
    }

    public async Task<SalidaInsumoDto> RegistrarAsync(RegistrarSalidaInsumoDto dto, string? registradoPor = null)
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
            Motivo = dto.Motivo,
            Observacion = dto.Observacion,
            RegistradoPor = registradoPor
        };

        insumo.StockActual -= dto.Cantidad;

        await _uow.SalidasInsumo.AgregarAsync(salida);
        await _uow.Insumos.ActualizarAsync(insumo);
        await _uow.GuardarCambiosAsync();

        return MapDto(salida, insumo.Nombre, insumo.Unidad);
    }

    public async Task<IEnumerable<SalidaInsumoDto>> RegistrarLoteAsync(
        RegistrarSalidaLoteDto dto, string registradoPor)
    {
        if (dto.Lineas.Count == 0)
            throw new InvalidOperationException("El lote debe tener al menos una línea");

        // Verificar stock de todos antes de descontar ninguno
        foreach (var linea in dto.Lineas)
        {
            var insumo = await _uow.Insumos.ObtenerPorIdAsync(linea.InsumoId)
                ?? throw new Exception($"Insumo {linea.InsumoId} no encontrado");
            if (insumo.StockActual < linea.Cantidad)
                throw new InvalidOperationException(
                    $"Stock insuficiente de {insumo.Nombre}. Disponible: {insumo.StockActual} {insumo.Unidad}");
        }

        var resultados = new List<SalidaInsumoDto>();
        foreach (var linea in dto.Lineas)
        {
            var resultado = await RegistrarAsync(new RegistrarSalidaInsumoDto
            {
                InsumoId = linea.InsumoId,
                Cantidad = linea.Cantidad,
                Motivo = dto.Motivo,
                Observacion = dto.Observacion,
            }, registradoPor);
            resultados.Add(resultado);
        }
        return resultados;
    }

    private static SalidaInsumoDto MapDto(SalidaInsumo s, string? nombre = null, string? unidad = null) => new()
    {
        SalidaInsumoId = s.SalidaInsumoId,
        InsumoId = s.InsumoId,
        NombreInsumo = nombre ?? s.Insumo?.Nombre ?? string.Empty,
        Unidad = unidad ?? s.Insumo?.Unidad,
        Cantidad = s.Cantidad,
        Fecha = s.Fecha,
        Motivo = s.Motivo,
        Observacion = s.Observacion,
        RegistradoPor = s.RegistradoPor
    };
}
