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

    /// <summary>Ventas cobradas por Yape/Plin. mes = "YYYY-MM"</summary>
    [HttpGet("pago-qr")]
    public async Task<IActionResult> GetPagoQr([FromQuery] string mes)
    {
        if (string.IsNullOrWhiteSpace(mes))
            mes = DateTime.UtcNow.ToString("yyyy-MM");

        var resultado = await _service.ReportePagoQrAsync(mes);
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
}
