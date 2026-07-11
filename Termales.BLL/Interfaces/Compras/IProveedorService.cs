using Termales.Common.DTOs.Compras;
using Termales.Common.Wrappers;

namespace Termales.BLL.Interfaces.Compras;

public interface IProveedorService
{
    Task<ApiResponse<ProveedorDto>> ObtenerPorIdAsync(int id);
    Task<ApiResponse<ProveedorDto>> ObtenerPorRucAsync(string ruc);
    Task<PagedResponse<ProveedorDto>> ObtenerPaginadoAsync(int pagina, int tamanoPagina, string? busqueda);
    Task<ApiResponse<ProveedorDto>> CrearAsync(CrearProveedorDto dto);
    Task<ApiResponse<ProveedorDto>> ActualizarAsync(ActualizarProveedorDto dto);
    Task<ApiResponse> DesactivarAsync(int id);
}
