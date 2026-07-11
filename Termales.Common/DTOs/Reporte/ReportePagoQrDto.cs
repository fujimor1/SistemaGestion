namespace Termales.Common.DTOs.Reporte;

public class ReportePagoQrDto
{
    public string Mes { get; set; } = string.Empty;
    public int TotalTransacciones { get; set; }
    public decimal MontoTotal { get; set; }
    public List<DetalleComprobanteReporteDto> Detalle { get; set; } = [];
}
