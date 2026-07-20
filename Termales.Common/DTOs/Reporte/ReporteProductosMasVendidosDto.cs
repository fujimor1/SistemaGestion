namespace Termales.Common.DTOs.Reporte;

public class ReporteProductosMasVendidosDto
{
    public string Desde { get; set; } = string.Empty;
    public string Hasta { get; set; } = string.Empty;
    public List<ProductoMasVendidoDto> Detalle { get; set; } = [];
}

public class ProductoMasVendidoDto
{
    public string Descripcion { get; set; } = string.Empty;
    public int CantidadVendida { get; set; }
    public decimal MontoTotal { get; set; }
}
