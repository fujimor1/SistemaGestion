using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Termales.API.Authorization;
using Termales.BLL.Interfaces;

namespace Termales.API.Controllers;

[ApiController]
[Route("api/reportes")]
[Authorize(Roles = Modulos.Operaciones)]
public class ReportesController : ControllerBase
{
    private readonly IReporteService _service;

    public ReportesController(IReporteService service) => _service = service;

    /// <summary>
    /// Reporte mensual de comprobantes (NV, BI, FI, NC) con resumen diario y detalle.
    /// mes = "YYYY-MM"
    /// </summary>
    [HttpGet("comprobantes")]
    public async Task<IActionResult> GetComprobantes([FromQuery] string mes)
    {
        if (string.IsNullOrWhiteSpace(mes))
            mes = DateTime.UtcNow.ToString("yyyy-MM");

        var resultado = await _service.ReporteComprobantesAsync(mes);
        return Ok(resultado);
    }

    /// <summary>
    /// Ventas netas por día para un rango de fechas arbitrario (no necesariamente un mes calendario).
    /// desde/hasta = "YYYY-MM-DD"
    /// </summary>
    [HttpGet("ventas-por-rango")]
    public async Task<IActionResult> GetVentasPorRango([FromQuery] string desde, [FromQuery] string hasta)
    {
        var resultado = await _service.ReporteVentasPorRangoAsync(desde, hasta);
        return Ok(resultado);
    }

    /// <summary>
    /// Reporte mensual de caja: apertura, ventas, egresos, cierre y cuadre por día.
    /// mes = "YYYY-MM"
    /// </summary>
    [HttpGet("caja")]
    public async Task<IActionResult> GetCaja([FromQuery] string mes)
    {
        if (string.IsNullOrWhiteSpace(mes))
            mes = DateTime.UtcNow.ToString("yyyy-MM");

        var resultado = await _service.ReporteCajaAsync(mes);
        return Ok(resultado);
    }

    /// <summary>
    /// Registro de Compras (SUNAT): compras del mes con RUC/razón social del proveedor y desglose de IGV.
    /// mes = "YYYY-MM"
    /// </summary>
    [HttpGet("compras")]
    public async Task<IActionResult> GetCompras([FromQuery] string mes)
    {
        if (string.IsNullOrWhiteSpace(mes))
            mes = DateTime.UtcNow.ToString("yyyy-MM");

        var resultado = await _service.ReporteComprasAsync(mes);
        return Ok(resultado);
    }

    /// <summary>Stock actual valorizado por insumo (StockActual × PrecioReferencia).</summary>
    [HttpGet("inventario")]
    public async Task<IActionResult> GetInventario()
    {
        var resultado = await _service.ReporteInventarioAsync();
        return Ok(resultado);
    }

    /// <summary>Ventas por categoría de menú (solo Comedor). mes = "YYYY-MM"</summary>
    [HttpGet("ventas-categoria")]
    public async Task<IActionResult> GetVentasCategoria([FromQuery] string mes)
    {
        if (string.IsNullOrWhiteSpace(mes))
            mes = DateTime.UtcNow.ToString("yyyy-MM");

        var resultado = await _service.ReporteVentasCategoriaAsync(mes);
        return Ok(resultado);
    }

    /// <summary>Productos/platos más vendidos, todas las ambientes. mes = "YYYY-MM"</summary>
    [HttpGet("productos-mas-vendidos")]
    public async Task<IActionResult> GetProductosMasVendidos([FromQuery] string mes)
    {
        if (string.IsNullOrWhiteSpace(mes))
            mes = DateTime.UtcNow.ToString("yyyy-MM");

        var resultado = await _service.ReporteProductosMasVendidosAsync(mes);
        return Ok(resultado);
    }

    /// <summary>Utilidad (ingreso - costo) de Comedor y Tienda. mes = "YYYY-MM"</summary>
    [HttpGet("utilidad")]
    public async Task<IActionResult> GetUtilidad([FromQuery] string mes)
    {
        if (string.IsNullOrWhiteSpace(mes))
            mes = DateTime.UtcNow.ToString("yyyy-MM");

        var resultado = await _service.ReporteUtilidadAsync(mes);
        return Ok(resultado);
    }

    /// <summary>Ventas por cajero/mesero. mes = "YYYY-MM"</summary>
    [HttpGet("personal")]
    public async Task<IActionResult> GetPersonal([FromQuery] string mes)
    {
        if (string.IsNullOrWhiteSpace(mes))
            mes = DateTime.UtcNow.ToString("yyyy-MM");

        var resultado = await _service.ReportePersonalAsync(mes);
        return Ok(resultado);
    }

    /// <summary>Listado de precios vigentes de todos los servicios/productos.</summary>
    [HttpGet("catalogo")]
    public async Task<IActionResult> GetCatalogo()
    {
        var resultado = await _service.ObtenerCatalogoAsync();
        return Ok(resultado);
    }

    /// <summary>Acumulado cobrado por Yape/Plin (incluye la porción QR de pagos Mixto)
    /// en un rango de fechas arbitrario. desde/hasta = "YYYY-MM-DD"</summary>
    [HttpGet("pago-qr")]
    public async Task<IActionResult> GetPagoQr([FromQuery] string desde, [FromQuery] string hasta)
    {
        var resultado = await _service.ReportePagoQrAsync(desde, hasta);
        return Ok(resultado);
    }

    /// <summary>Comandas del mes: cantidad, tiempos de atención por mesa. mes = "YYYY-MM"</summary>
    [HttpGet("comandas")]
    public async Task<IActionResult> GetComandas([FromQuery] string mes)
    {
        if (string.IsNullOrWhiteSpace(mes))
            mes = DateTime.UtcNow.ToString("yyyy-MM");

        var resultado = await _service.ReporteComandasAsync(mes);
        return Ok(resultado);
    }

    /// <summary>Insumos y productos por debajo de su stock mínimo configurado.</summary>
    [HttpGet("stock-minimo")]
    public async Task<IActionResult> GetStockMinimo()
    {
        var resultado = await _service.ReporteStockMinimoAsync();
        return Ok(resultado);
    }

    /// <summary>Resumen imprimible del día: todo lo vendido (con costo cuando se conoce) más el
    /// cuadre de caja, sin importar la forma de pago. fecha = "YYYY-MM-DD"</summary>
    [HttpGet("liquidacion-caja")]
    public async Task<IActionResult> GetLiquidacionCaja([FromQuery] string fecha)
    {
        if (string.IsNullOrWhiteSpace(fecha))
            fecha = DateTime.UtcNow.AddHours(-5).ToString("yyyy-MM-dd");

        var resultado = await _service.ReporteLiquidacionCajaAsync(fecha);
        return Ok(resultado);
    }
}
