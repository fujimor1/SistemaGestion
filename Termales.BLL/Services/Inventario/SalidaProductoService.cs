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
            Motivo = dto.Motivo,
            Observacion = dto.Observacion
        };

        producto.Stock -= dto.Cantidad;

        await _uow.SalidasProducto.AgregarAsync(salida);
        await _uow.Productos.ActualizarAsync(producto);
        await _uow.GuardarCambiosAsync();

        return MapDto(salida, producto.Nombre);
    }

    public async Task<IEnumerable<SalidaProductoDto>> RegistrarLoteAsync(RegistrarSalidaProductoLoteDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Motivo))
            throw new InvalidOperationException("El motivo es requerido.");

        var resultados = new List<SalidaProductoDto>();
        foreach (var linea in dto.Lineas)
        {
            var producto = await _uow.Productos.ObtenerPorIdAsync(linea.ProductoId)
                ?? throw new InvalidOperationException($"Producto {linea.ProductoId} no encontrado.");

            if (producto.Stock < linea.Cantidad)
                throw new InvalidOperationException(
                    $"Stock insuficiente para '{producto.Nombre}'. Disponible: {producto.Stock}");

            var salida = new SalidaProducto
            {
                ProductoId = linea.ProductoId,
                Cantidad = linea.Cantidad,
                Motivo = dto.Motivo,
                Observacion = dto.Observacion
            };

            producto.Stock -= linea.Cantidad;

            await _uow.SalidasProducto.AgregarAsync(salida);
            await _uow.Productos.ActualizarAsync(producto);
            resultados.Add(MapDto(salida, producto.Nombre));
        }

        await _uow.GuardarCambiosAsync();
        return resultados;
    }

    public async Task<IEnumerable<SalidaProductoDto>> ObtenerHistorialAsync(
        DateTime? desde, DateTime? hasta, int? productoId, string? motivo)
    {
        var salidas = await _uow.SalidasProducto.ObtenerHistorialAsync(desde, hasta, productoId, motivo);
        return salidas.Select(s => MapDto(s));
    }

    private static SalidaProductoDto MapDto(SalidaProducto s, string? nombre = null) => new()
    {
        SalidaProductoId = s.SalidaProductoId,
        ProductoId = s.ProductoId,
        NombreProducto = nombre ?? s.Producto?.Nombre ?? string.Empty,
        Cantidad = s.Cantidad,
        Fecha = s.Fecha,
        Motivo = s.Motivo,
        Observacion = s.Observacion
    };
}
