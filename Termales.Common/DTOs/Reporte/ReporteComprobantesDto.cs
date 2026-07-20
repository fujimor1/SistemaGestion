namespace Termales.Common.DTOs.Reporte;

public class ReporteComprobantesDto
{
    public string Desde                { get; set; } = string.Empty;
    public string Hasta                { get; set; } = string.Empty;
    public int    TotalEmitidos        { get; set; }
    public int    TotalAnulados        { get; set; }
    public int    TotalNV              { get; set; }
    public int    TotalBI              { get; set; }
    public int    TotalFI              { get; set; }
    public int    TotalNC              { get; set; }
    public decimal MontoTotalEmitido   { get; set; }
    public decimal MontoTotalAnulado   { get; set; }
    public decimal MontoNeto           { get; set; }
    public List<ResumenDiarioComprobanteDto>  PorDia  { get; set; } = [];
    public List<DetalleComprobanteReporteDto> Detalle { get; set; } = [];
}

public class ResumenDiarioComprobanteDto
{
    public DateOnly Fecha            { get; set; }
    public int      CantidadNV       { get; set; }
    public int      CantidadBI       { get; set; }
    public int      CantidadFI       { get; set; }
    public int      CantidadNC       { get; set; }
    public int      CantidadAnulados { get; set; }
    public decimal  MontoEmitido     { get; set; }
    public decimal  MontoAnulado     { get; set; }
    public decimal  MontoNeto        { get; set; }
}

public class DetalleComprobanteReporteDto
{
    public string   NumeroFormateado { get; set; } = string.Empty;
    public string   TipoComprobante  { get; set; } = string.Empty;
    public string   TipoAmbiente     { get; set; } = string.Empty;
    public string?  ClienteNombre    { get; set; }
    public decimal  Total            { get; set; }
    public string   Estado           { get; set; } = string.Empty;
    public DateTime FechaEmision     { get; set; }
    public string?  MotivoAnulacion  { get; set; }
    public string?  AutorizadoPor    { get; set; }
}
