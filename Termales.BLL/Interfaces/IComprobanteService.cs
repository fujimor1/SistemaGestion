using Termales.Common.DTOs.Comprobante;
using Termales.Common.DTOs.Sunat;
using Termales.Common.Wrappers;
using Termales.Entities.Enums;

namespace Termales.BLL.Interfaces;

public interface IComprobanteService
{
    Task<ApiResponse<ComprobanteResultadoDto>> GenerarComprobanteComedor(int ordenId, GenerarComprobanteComedorDto dto);
    Task<ApiResponse<ComprobanteResultadoDto>> GenerarComprobanteBanio(GenerarComprobanteBanioDto dto);
    Task<ApiResponse<ComprobanteResultadoDto>> GenerarComprobanteHabitacion(int habitacionId, GenerarComprobanteDto dto);
    Task<ApiResponse<ComprobanteResultadoDto>> GenerarComprobanteTienda(GenerarComprobanteTiendaDto dto);
    Task<IEnumerable<ComprobanteListadoDto>> ObtenerPorFechaAsync(string? fecha, string? tipoAmbiente);
    Task<IEnumerable<ComprobanteListadoDto>> ObtenerPendientesDeCobroAsync();
    Task<ApiResponse> MarcarCobradoAsync(int comprobanteId, MetodoPago metodoPagoReal);
    Task<ApiResponse> ActualizarMetodoPagoAsync(int comprobanteId, ActualizarMetodoPagoDto dto);
    Task<ApiResponse> SolicitarAnulacionAsync(int id, string motivo, string cajero);
    Task<ApiResponse<ComprobanteResultadoDto>> EmitirNotaCreditoAsync(int comprobanteOrigenId, EmitirNotaCreditoDto dto);
    Task<IEnumerable<AnulacionListadoDto>> ObtenerAnulacionesAsync(string? desde, string? hasta);
    Task<ApiResponse<ComprobanteDetalleCompletoDto>> ObtenerDetalleAsync(int comprobanteId);
    Task<ApiResponse<ResultadoEmisionSunatDto>> ReenviarSunatAsync(int comprobanteId);
    Task<IEnumerable<ComprobanteSunatPendienteDto>> ObtenerPendientesSunatAsync();
    Task<IEnumerable<ComprobanteElectronicoDto>> ObtenerFacturasBoletasAsync(string? fecha);
    Task<IEnumerable<NotaCreditoListadoDto>> ObtenerNotasCreditoAsync(string? desde, string? hasta);
}
