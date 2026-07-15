using Termales.Common.DTOs.Tienda;
using Termales.Common.Wrappers;

namespace Termales.BLL.Interfaces.Tienda;

public interface IProductoService
{
    Task<ApiResponse<IEnumerable<ProductoDto>>> ObtenerTodosAsync();
    Task<ApiResponse<IEnumerable<ProductoDto>>> ObtenerTodosParaGestionAsync();
    Task<ApiResponse<(IEnumerable<ProductoDto> Items, int Total)>> ObtenerPaginadoAsync(int pagina, int tamanoPagina, string? busqueda);
    Task<ApiResponse<ProductoDto>> ObtenerPorIdAsync(int id);
    Task<ApiResponse<ProductoDto>> ObtenerPorCodigoBarrasAsync(string codigoBarras);
    Task<ApiResponse<ProductoDto>> CrearAsync(CrearProductoDto dto);
    Task<ApiResponse<ProductoDto>> ActualizarAsync(int id, ActualizarProductoDto dto);
    Task<ApiResponse<bool>> EliminarAsync(int id);
}
