namespace Termales.Common.DTOs.Reporte;

public class ReportePersonalDto
{
    public string Desde { get; set; } = string.Empty;
    public string Hasta { get; set; } = string.Empty;
    public decimal MontoTotal { get; set; }
    public List<VentasPorCajeroDto> Detalle { get; set; } = [];
}

public class VentasPorCajeroDto
{
    public string Cajero { get; set; } = string.Empty;
    public int CantidadVentas { get; set; }
    public decimal MontoTotal { get; set; }
}
