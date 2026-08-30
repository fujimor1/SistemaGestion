using Termales.Common.DTOs.Reporte;

namespace Termales.BLL.Interfaces;

public interface IReporteService
{
    Task<ReporteComprobantesDto> ReporteComprobantesAsync(string desde, string hasta);
    Task<List<ResumenDiarioComprobanteDto>> ReporteVentasPorRangoAsync(string desde, string hasta);
    Task<ReporteCajaDto>         ReporteCajaAsync(string desde, string hasta, string? cajero = null);
    Task<RegistroComprasDto>     ReporteComprasAsync(string desde, string hasta);
    Task<ReporteInventarioDto>   ReporteInventarioAsync();
    Task<List<MovimientoInventarioDto>> ReporteMovimientosInventarioAsync();
    Task<ReporteVentasCategoriaDto> ReporteVentasCategoriaAsync(string desde, string hasta);
    Task<ReporteProductosMasVendidosDto> ReporteProductosMasVendidosAsync(string desde, string hasta, string? cajero = null);
    Task<ReporteUtilidadDto>     ReporteUtilidadAsync(string desde, string hasta);
    Task<ReportePersonalDto>     ReportePersonalAsync(string desde, string hasta);
    Task<CatalogoDto>            ObtenerCatalogoAsync();
    Task<ReportePagoQrDto>       ReportePagoQrAsync(string desde, string hasta, string? cajero = null);
    Task<ReporteComandasDto>     ReporteComandasAsync(string desde, string hasta);
    Task<ReporteStockMinimoDto>  ReporteStockMinimoAsync();
    Task<LiquidacionCajaDto>     ReporteLiquidacionCajaAsync(string fecha, string? cajero = null);
    Task<List<string>>           ObtenerCajerosAsync();
}
