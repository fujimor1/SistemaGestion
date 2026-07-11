namespace Termales.Common.DTOs.Reporte;

public class ReporteStockMinimoDto
{
    public List<StockBajoDto> Insumos { get; set; } = [];
    public List<StockBajoDto> Productos { get; set; } = [];
}

public class StockBajoDto
{
    public string Nombre { get; set; } = string.Empty;
    public string? Unidad { get; set; }
    public decimal StockActual { get; set; }
    public decimal StockMinimo { get; set; }
}
