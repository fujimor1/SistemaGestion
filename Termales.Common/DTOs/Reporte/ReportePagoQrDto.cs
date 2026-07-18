namespace Termales.Common.DTOs.Reporte;

public class ReportePagoQrDto
{
    public string Desde { get; set; } = string.Empty;
    public string Hasta { get; set; } = string.Empty;
    public int TotalTransacciones { get; set; }
    public decimal MontoTotal { get; set; }
    public List<DetallePagoQrDto> Detalle { get; set; } = [];
}

public class DetallePagoQrDto
{
    public string   NumeroFormateado { get; set; } = string.Empty;
    public string   TipoComprobante  { get; set; } = string.Empty;
    public string   TipoAmbiente     { get; set; } = string.Empty;
    public string?  ClienteNombre    { get; set; }
    /// <summary>Monto realmente cobrado por Yape/Plin: el total si el pago fue
    /// 100% QR, o solo la porción QR si fue Mixto (Total - MontoEfectivoMixto).</summary>
    public decimal  MontoYape        { get; set; }
    public bool     EsMixto          { get; set; }
    public DateTime FechaEmision     { get; set; }
}
