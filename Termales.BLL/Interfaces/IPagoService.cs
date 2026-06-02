using Termales.Common.DTOs;
using Termales.Common.Wrappers;

namespace Termales.BLL.Interfaces;

public interface IPagoService
{
    Task<ApiResponse<PagoDto>> ObtenerPorReservaAsync(int reservaId);
    Task<ApiResponse<PagoDto>> RegistrarPagoAsync(RegistrarPagoDto dto);
    Task<ApiResponse<decimal>> ObtenerTotalRecaudadoAsync(DateTime desde, DateTime hasta);
}
