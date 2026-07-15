using Termales.Common.DTOs.Comprobante;
using Termales.Common.Wrappers;

namespace Termales.BLL.Interfaces;

/// <summary>
/// Emisión de Notas de Crédito, extraída de <see cref="IComprobanteService"/> para que tanto la
/// emisión directa (endpoint) como el flujo de solicitud/aprobación de anulación puedan usarla sin
/// crear una dependencia circular entre ambos servicios.
/// </summary>
public interface INotaCreditoService
{
    Task<ApiResponse<ComprobanteResultadoDto>> EmitirAsync(int comprobanteOrigenId, EmitirNotaCreditoDto dto, string cajero);
}
