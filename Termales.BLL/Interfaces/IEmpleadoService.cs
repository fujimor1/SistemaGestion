using Termales.Common.DTOs;
using Termales.Common.Wrappers;

namespace Termales.BLL.Interfaces;

public interface IEmpleadoService
{
    Task<ApiResponse<EmpleadoDto>> ObtenerPorIdAsync(int id);
    Task<ApiResponse<EmpleadoDto>> ObtenerPorDniAsync(string dni);
    Task<PagedResponse<EmpleadoDto>> ObtenerPaginadoAsync(int pagina, int tamanoPagina, string? busqueda);
    Task<ApiResponse<EmpleadoDto>> CrearAsync(CrearEmpleadoDto dto);
    Task<ApiResponse<EmpleadoDto>> ActualizarAsync(ActualizarEmpleadoDto dto);
    Task<ApiResponse> DesactivarAsync(int id);
}
