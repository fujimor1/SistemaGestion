using Termales.BLL.Interfaces.Tienda;
using Termales.Common.DTOs.Tienda;
using Termales.Common.Wrappers;
using Termales.DAL.UnitOfWork;
using Termales.Entities.Models.Tienda;

namespace Termales.BLL.Services.Tienda;

public class ProductoService : IProductoService
{
    private readonly IUnitOfWork _uow;

    public ProductoService(IUnitOfWork uow) => _uow = uow;

    public async Task<ApiResponse<IEnumerable<ProductoDto>>> ObtenerTodosAsync()
    {
        var productos = await _uow.Productos.ObtenerActivosAsync();
        return ApiResponse<IEnumerable<ProductoDto>>.Exitoso(productos.Select(MapDto));
    }

    public async Task<ApiResponse<IEnumerable<ProductoDto>>> ObtenerTodosParaGestionAsync()
    {
        var productos = await _uow.Productos.ObtenerTodosParaGestionAsync();
        return ApiResponse<IEnumerable<ProductoDto>>.Exitoso(productos.Select(MapDto));
    }

    public async Task<ApiResponse<(IEnumerable<ProductoDto> Items, int Total)>> ObtenerPaginadoAsync(
        int pagina, int tamanoPagina, string? busqueda)
    {
        var (items, total) = await _uow.Productos.ObtenerPaginadoAsync(pagina, tamanoPagina, busqueda);
        return ApiResponse<(IEnumerable<ProductoDto>, int)>.Exitoso((items.Select(MapDto), total));
    }

    public async Task<ApiResponse<ProductoDto>> ObtenerPorIdAsync(int id)
    {
        var p = await _uow.Productos.ObtenerPorIdAsync(id);
        return p is null
            ? ApiResponse<ProductoDto>.Fallido("Producto no encontrado")
            : ApiResponse<ProductoDto>.Exitoso(MapDto(p));
    }

    public async Task<ApiResponse<ProductoDto>> ObtenerPorCodigoBarrasAsync(string codigoBarras)
    {
        var p = await _uow.Productos.ObtenerPorCodigoBarrasAsync(codigoBarras);
        return p is null
            ? ApiResponse<ProductoDto>.Fallido("Producto no encontrado")
            : ApiResponse<ProductoDto>.Exitoso(MapDto(p));
    }

    public async Task<ApiResponse<ProductoDto>> CrearAsync(CrearProductoDto dto)
    {
        if (!string.IsNullOrWhiteSpace(dto.CodigoBarras))
        {
            var existente = await _uow.Productos.ObtenerPorCodigoBarrasAsync(dto.CodigoBarras);
            if (existente is not null)
                return ApiResponse<ProductoDto>.Fallido($"Ya existe un producto con el código de barras '{dto.CodigoBarras}'");
        }

        var producto = new Producto
        {
            Nombre       = dto.Nombre.Trim(),
            Descripcion  = string.IsNullOrWhiteSpace(dto.Descripcion) ? "----" : dto.Descripcion.Trim(),
            CodigoBarras = dto.CodigoBarras?.Trim(),
            Precio       = dto.Precio,
            Stock        = dto.Stock,
            StockMinimo  = dto.StockMinimo,
        };

        await _uow.Productos.AgregarAsync(producto);
        await _uow.GuardarCambiosAsync();

        return ApiResponse<ProductoDto>.Exitoso(MapDto(producto), "Producto creado exitosamente");
    }

    public async Task<ApiResponse<ProductoDto>> ActualizarAsync(int id, ActualizarProductoDto dto)
    {
        var producto = await _uow.Productos.ObtenerPorIdAsync(id);
        if (producto is null)
            return ApiResponse<ProductoDto>.Fallido("Producto no encontrado");

        if (!string.IsNullOrWhiteSpace(dto.CodigoBarras) && dto.CodigoBarras != producto.CodigoBarras)
        {
            var existente = await _uow.Productos.ObtenerPorCodigoBarrasAsync(dto.CodigoBarras);
            if (existente is not null && existente.ProductoId != id)
                return ApiResponse<ProductoDto>.Fallido($"Ya existe un producto con el código de barras '{dto.CodigoBarras}'");
        }

        producto.Nombre       = dto.Nombre.Trim();
        producto.Descripcion  = string.IsNullOrWhiteSpace(dto.Descripcion) ? "----" : dto.Descripcion.Trim();
        producto.CodigoBarras = dto.CodigoBarras?.Trim();
        producto.Precio       = dto.Precio;
        producto.Stock        = dto.Stock;
        producto.StockMinimo  = dto.StockMinimo;
        producto.Activo       = dto.Activo;

        await _uow.Productos.ActualizarAsync(producto);
        await _uow.GuardarCambiosAsync();

        return ApiResponse<ProductoDto>.Exitoso(MapDto(producto), "Producto actualizado");
    }

    public async Task<ApiResponse<bool>> EliminarAsync(int id)
    {
        var producto = await _uow.Productos.ObtenerPorIdAsync(id);
        if (producto is null)
            return ApiResponse<bool>.Fallido("Producto no encontrado");

        producto.Activo = false;
        await _uow.Productos.ActualizarAsync(producto);
        await _uow.GuardarCambiosAsync();

        return ApiResponse<bool>.Exitoso(true, "Producto eliminado");
    }

    private static ProductoDto MapDto(Producto p) => new()
    {
        ProductoId    = p.ProductoId,
        Nombre        = p.Nombre,
        Descripcion   = p.Descripcion,
        CodigoBarras  = p.CodigoBarras,
        Precio        = p.Precio,
        Stock         = p.Stock,
        StockMinimo   = p.StockMinimo,
        Activo        = p.Activo,
        FechaRegistro = p.FechaRegistro,
    };
}
