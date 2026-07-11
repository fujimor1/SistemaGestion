using Termales.BLL.Interfaces.Inventario;
using Termales.Common.DTOs.Inventario;
using Termales.DAL.UnitOfWork;
using Termales.Entities.Models.Inventario;

namespace Termales.BLL.Services.Inventario;

public class EntradaProductoService : IEntradaProductoService
{
    private readonly IUnitOfWork _uow;

    public EntradaProductoService(IUnitOfWork uow) => _uow = uow;

    public async Task<IEnumerable<EntradaProductoDto>> ObtenerPorProductoAsync(int productoId)
    {
        var entradas = await _uow.EntradasProducto.ObtenerPorProductoAsync(productoId);
        return entradas.Select(e => new EntradaProductoDto
        {
            EntradaProductoId = e.EntradaProductoId,
            ProductoId = e.ProductoId,
            NombreProducto = e.Producto?.Nombre ?? string.Empty,
            Cantidad = e.Cantidad,
            PrecioUnitario = e.PrecioUnitario,
            Total = e.Total,
            Fecha = e.Fecha,
            Observacion = e.Observacion
        });
    }

    public async Task<EntradaProductoDto> RegistrarAsync(RegistrarEntradaProductoDto dto)
    {
        var producto = await _uow.Productos.ObtenerPorIdAsync(dto.ProductoId)
            ?? throw new Exception($"Producto {dto.ProductoId} no encontrado");

        var entrada = new EntradaProducto
        {
            ProductoId = dto.ProductoId,
            Cantidad = dto.Cantidad,
            PrecioUnitario = dto.PrecioUnitario,
            Total = dto.Cantidad * dto.PrecioUnitario,
            Observacion = dto.Observacion
        };

        producto.Stock += dto.Cantidad;
        producto.PrecioCompra = dto.PrecioUnitario;

        await _uow.EntradasProducto.AgregarAsync(entrada);
        await _uow.Productos.ActualizarAsync(producto);
        await _uow.GuardarCambiosAsync();

        return new EntradaProductoDto
        {
            EntradaProductoId = entrada.EntradaProductoId,
            ProductoId = entrada.ProductoId,
            NombreProducto = producto.Nombre,
            Cantidad = entrada.Cantidad,
            PrecioUnitario = entrada.PrecioUnitario,
            Total = entrada.Total,
            Fecha = entrada.Fecha,
            Observacion = entrada.Observacion
        };
    }
}
