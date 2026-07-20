namespace Termales.Common.DTOs.Reporte;

public class ReporteComandasDto
{
    public string Desde { get; set; } = string.Empty;
    public string Hasta { get; set; } = string.Empty;
    public int TotalComandas { get; set; }
    public decimal TiempoPromedioMinutos { get; set; }
    public List<ComandaDetalleDto> Detalle { get; set; } = [];
}

public class ComandaDetalleDto
{
    public int OrdenId { get; set; }
    public int NumeroMesa { get; set; }
    public DateTime FechaApertura { get; set; }
    public DateTime? FechaCierre { get; set; }
    public decimal? DuracionMinutos { get; set; }
    public int CantidadItems { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; } = string.Empty;
}
