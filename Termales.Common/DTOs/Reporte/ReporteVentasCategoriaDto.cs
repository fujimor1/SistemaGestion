namespace Termales.Common.DTOs.Reporte;

public class ReporteVentasCategoriaDto
{
    public string Desde { get; set; } = string.Empty;
    public string Hasta { get; set; } = string.Empty;
    public decimal MontoTotal { get; set; }
    /// <summary>Solo cubre Comedor por ahora — Tienda no tiene categoría de producto.</summary>
    public List<VentaCategoriaDto> Detalle { get; set; } = [];
}

public class VentaCategoriaDto
{
    public string Categoria { get; set; } = string.Empty;
    public int CantidadVendida { get; set; }
    public decimal MontoTotal { get; set; }
}
