namespace Termales.Common.DTOs.Reporte;

public class ReporteCuentasPorCobrarDto
{
    public int TotalPendientes { get; set; }
    public decimal MontoTotal { get; set; }
    public List<CuentaPorCobrarDto> Detalle { get; set; } = [];
}

public class CuentaPorCobrarDto
{
    public int ComprobanteId { get; set; }
    public string Numero { get; set; } = string.Empty;
    public string TipoComprobante { get; set; } = string.Empty;
    public string TipoAmbiente { get; set; } = string.Empty;
    public string? ClienteNombre { get; set; }
    public string? ClienteDocumento { get; set; }
    public string? Cajero { get; set; }
    public decimal Total { get; set; }
    public DateTime FechaEmision { get; set; }
    public int DiasEnDeuda { get; set; }
    public string Estado { get; set; } = string.Empty;
}
