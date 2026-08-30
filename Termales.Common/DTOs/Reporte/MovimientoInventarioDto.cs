namespace Termales.Common.DTOs.Reporte;

/// <summary>
/// Un movimiento de inventario (alta de artículo, entrada por compra, o salida por
/// consumo), unificado entre Insumos (Comedor/Baños/Hospedaje) y Productos (Tienda)
/// para exportarlo como una sola línea de tiempo — ver ReporteService.ReporteMovimientosInventarioAsync.
/// </summary>
public class MovimientoInventarioDto
{
    public DateTime Fecha { get; set; }
    /// <summary>"Alta" | "Entrada" | "Salida"</summary>
    public string Tipo { get; set; } = string.Empty;
    /// <summary>"tienda" | "comedor" | "banio" | "habitacion"</summary>
    public string Categoria { get; set; } = string.Empty;
    /// <summary>"insumo" | "activo" | "producto"</summary>
    public string TipoArticulo { get; set; } = string.Empty;
    public string Articulo { get; set; } = string.Empty;
    public string? Unidad { get; set; }
    public decimal Cantidad { get; set; }
    public decimal? PrecioUnitario { get; set; }
    public decimal? Total { get; set; }
    public string? Observacion { get; set; }
}
