using Termales.BLL.Interfaces.Inventario;
using Termales.Common.DTOs.Inventario;
using Termales.DAL.UnitOfWork;
using Termales.Entities.Models.Inventario;

namespace Termales.BLL.Services.Inventario;

public class EntradaInsumoService : IEntradaInsumoService
{
    private readonly IUnitOfWork _uow;

    public EntradaInsumoService(IUnitOfWork uow) => _uow = uow;

    public async Task<IEnumerable<EntradaInsumoDto>> ObtenerPorInsumoAsync(int insumoId)
    {
        var entradas = await _uow.EntradasInsumo.ObtenerPorInsumoAsync(insumoId);
        return entradas.Select(e => new EntradaInsumoDto
        {
            EntradaInsumoId = e.EntradaInsumoId,
            InsumoId = e.InsumoId,
            NombreInsumo = e.Insumo?.Nombre ?? string.Empty,
            Cantidad = e.Cantidad,
            PrecioUnitario = e.PrecioUnitario,
            Total = e.Total,
            Fecha = e.Fecha,
            Observacion = e.Observacion
        });
    }

    public async Task<EntradaInsumoDto> RegistrarAsync(RegistrarEntradaInsumoDto dto)
    {
        var insumo = await _uow.Insumos.ObtenerPorIdAsync(dto.InsumoId)
            ?? throw new Exception($"Insumo {dto.InsumoId} no encontrado");

        var entrada = new EntradaInsumo
        {
            InsumoId = dto.InsumoId,
            Cantidad = dto.Cantidad,
            PrecioUnitario = dto.PrecioUnitario,
            Total = dto.Cantidad * dto.PrecioUnitario,
            Observacion = dto.Observacion
        };

        insumo.StockActual += dto.Cantidad;
        insumo.PrecioReferencia = dto.PrecioUnitario;

        await _uow.EntradasInsumo.AgregarAsync(entrada);
        await _uow.Insumos.ActualizarAsync(insumo);
        await _uow.GuardarCambiosAsync();

        return new EntradaInsumoDto
        {
            EntradaInsumoId = entrada.EntradaInsumoId,
            InsumoId = entrada.InsumoId,
            NombreInsumo = insumo.Nombre,
            Cantidad = entrada.Cantidad,
            PrecioUnitario = entrada.PrecioUnitario,
            Total = entrada.Total,
            Fecha = entrada.Fecha,
            Observacion = entrada.Observacion
        };
    }
}
