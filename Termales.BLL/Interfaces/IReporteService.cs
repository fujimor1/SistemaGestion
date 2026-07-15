using Termales.Common.DTOs.Reporte;

namespace Termales.BLL.Interfaces;

public interface IReporteService
{
    Task<ReporteComprobantesDto> ReporteComprobantesAsync(string mes);
    Task<List<ResumenDiarioComprobanteDto>> ReporteVentasPorRangoAsync(string desde, string hasta);
    Task<ReporteCajaDto>         ReporteCajaAsync(string mes);
    Task<RegistroComprasDto>     ReporteComprasAsync(string mes);
    Task<ReporteInventarioDto>   ReporteInventarioAsync();
    Task<ReporteVentasCategoriaDto> ReporteVentasCategoriaAsync(string mes);
    Task<ReporteProductosMasVendidosDto> ReporteProductosMasVendidosAsync(string mes);
    Task<ReporteUtilidadDto>     ReporteUtilidadAsync(string mes);
    Task<ReportePersonalDto>     ReportePersonalAsync(string mes);
    Task<CatalogoDto>            ObtenerCatalogoAsync();
    Task<ReportePagoQrDto>       ReportePagoQrAsync(string mes);
    Task<ReporteComandasDto>     ReporteComandasAsync(string mes);
    Task<ReporteStockMinimoDto>  ReporteStockMinimoAsync();
    Task<LiquidacionCajaDto>     ReporteLiquidacionCajaAsync(string fecha);
}
