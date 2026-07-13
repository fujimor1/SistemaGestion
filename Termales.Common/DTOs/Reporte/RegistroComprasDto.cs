namespace Termales.Common.DTOs.Reporte;

public class RegistroComprasDto
{
    public string Mes { get; set; } = string.Empty;
    public int TotalRegistros { get; set; }
    public decimal MontoTotalGravada { get; set; }
    public decimal MontoTotalIgv { get; set; }
    public decimal MontoTotal { get; set; }
    public List<DetalleCompraReporteDto> Detalle { get; set; } = [];
}

public class DetalleCompraReporteDto
{
    public string Ruc { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string TipoComprobante { get; set; } = string.Empty;
    public string? Serie { get; set; }
    public int? Numero { get; set; }
    public DateTime FechaEmision { get; set; }
    public decimal TotalGravada { get; set; }
    public decimal Igv { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; } = string.Empty;
}
