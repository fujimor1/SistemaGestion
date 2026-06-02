using Termales.Common.DTOs;
using Termales.Common.Wrappers;

namespace Termales.BLL.Interfaces;

public interface IClienteService
{
    Task<ApiResponse<ClienteDto>> ObtenerPorIdAsync(int id);
    Task<ApiResponse<ClienteDto>> ObtenerPorDniAsync(string dni);
    Task<PagedResponse<ClienteDto>> ObtenerPaginadoAsync(int pagina, int tamanoPagina, string? busqueda);
    Task<ApiResponse<ClienteDto>> CrearAsync(CrearClienteDto dto);
    Task<ApiResponse<ClienteDto>> ActualizarAsync(ActualizarClienteDto dto);
    Task<ApiResponse> DesactivarAsync(int id);
}
