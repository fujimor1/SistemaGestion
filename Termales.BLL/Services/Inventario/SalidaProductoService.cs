using Termales.BLL.Interfaces.Inventario;
using Termales.Common.DTOs.Inventario;
using Termales.DAL.UnitOfWork;
using Termales.Entities.Models.Inventario;

namespace Termales.BLL.Services.Inventario;

public class SalidaProductoService : ISalidaProductoService
{
    private readonly IUnitOfWork _uow;

    public SalidaProductoService(IUnitOfWork uow) => _uow = uow;

    public async Task<IEnumerable<SalidaProductoDto>> ObtenerPorProductoAsync(int productoId)
    {
        var salidas = await _uow.SalidasProducto.ObtenerPorProductoAsync(productoId);
        return salidas.Select(s => MapDto(s));
    }

    public async Task<SalidaProductoDto> RegistrarAsync(RegistrarSalidaProductoDto dto)
    {
        var producto = await _uow.Productos.ObtenerPorIdAsync(dto.ProductoId)
            ?? throw new Exception($"Producto {dto.ProductoId} no encontrado");

        if (producto.Stock < dto.Cantidad)
            throw new InvalidOperationException(
                $"Stock insuficiente. Disponible: {producto.Stock}");

        if (dto.Observacion?.Length > 500)
            throw new InvalidOperationException(
                "La observacion no puede superar los 500 caracteres.");

        var salida = new SalidaProducto
        {
            ProductoId = dto.ProductoId,
            Cantidad = dto.Cantidad,
            Observacion = dto.Observacion
        };

        producto.Stock -= dto.Cantidad;

        await _uow.SalidasProducto.AgregarAsync(salida);
        await _uow.Productos.ActualizarAsync(producto);
        await _uow.GuardarCambiosAsync();

        return MapDto(salida, producto.Nombre);
    }

    private static SalidaProductoDto MapDto(SalidaProducto s, string? nombre = null) => new()
    {
        SalidaProductoId = s.SalidaProductoId,
        ProductoId = s.ProductoId,
        NombreProducto = nombre ?? s.Producto?.Nombre ?? string.Empty,
        Cantidad = s.Cantidad,
        Fecha = s.Fecha,
        Observacion = s.Observacion
    };
}
