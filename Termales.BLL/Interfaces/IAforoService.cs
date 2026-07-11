using Termales.Common.DTOs;
using Termales.Common.Wrappers;

namespace Termales.BLL.Interfaces;

public interface IAforoService
{
    Task<ApiResponse<AforoDto>> ObtenerPorIdAsync(int id);
    Task<ApiResponse<IEnumerable<AforoDto>>> ObtenerPorFechaAsync(DateTime fecha);
    Task<ApiResponse<AforoDto>> ObtenerPorTipoYFechaAsync(int tipoServicioId, DateTime fecha);
    Task<ApiResponse<AforoDto>> CrearAsync(CrearAforoDto dto);
    Task<ApiResponse<AforoDto>> ActualizarAsync(ActualizarAforoDto dto);
}
