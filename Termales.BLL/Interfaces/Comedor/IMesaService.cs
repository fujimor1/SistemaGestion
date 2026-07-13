using Termales.Common.DTOs.Comedor;
using Termales.Common.Wrappers;

namespace Termales.BLL.Interfaces.Comedor;

public interface IMesaService
{
    Task<ApiResponse<IEnumerable<MesaDto>>> ObtenerTodasAsync();
    Task<ApiResponse<MesaDto>> ObtenerPorIdAsync(int id);
    Task<ApiResponse<MesaDto>> CrearAsync(CrearMesaDto dto);
    Task<ApiResponse<MesaDto>> ActualizarAsync(ActualizarMesaDto dto);
    Task<ApiResponse> DesactivarAsync(int id);
    Task<ApiResponse> UnirAsync(int mesaPrincipalId, int mesaSecundariaId);
    Task<ApiResponse> SepararAsync(int mesaId);
}
