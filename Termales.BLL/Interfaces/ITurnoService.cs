using Termales.Common.DTOs;
using Termales.Common.Wrappers;

namespace Termales.BLL.Interfaces;

public interface ITurnoService
{
    Task<ApiResponse<TurnoDto>> RegistrarIngresoAsync(RegistrarTurnoDto dto);
    Task<ApiResponse<DisponibilidadDto>> VerificarDisponibilidadAsync(int tipoServicioId, DateTime fecha, int cantidadPersonas);
    Task<ApiResponse<IEnumerable<DisponibilidadDto>>> ObtenerAforoDelDiaAsync(DateTime fecha);
    Task<ApiResponse<TurnoDto>> ObtenerPorIdAsync(int id);
    Task<ApiResponse<IEnumerable<TurnoDto>>> ObtenerPorTipoYFechaAsync(int tipoServicioId, DateTime fecha);
    Task<ApiResponse<IEnumerable<TipoServicioDto>>> ObtenerTiposServicioAsync();
}
