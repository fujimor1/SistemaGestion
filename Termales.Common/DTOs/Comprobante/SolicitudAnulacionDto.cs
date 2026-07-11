namespace Termales.Common.DTOs.Comprobante;

public class SolicitudAnulacionDto
{
    public int    SolicitudAnulacionId      { get; set; }
    public int    ComprobanteId             { get; set; }
    public string NumeroFormateado          { get; set; } = string.Empty;
    public string TipoComprobante           { get; set; } = string.Empty;
    public string TipoAmbiente              { get; set; } = string.Empty;
    public string? ClienteNombre            { get; set; }
    public decimal Total                    { get; set; }
    public string Motivo                    { get; set; } = string.Empty;
    public string SolicitadoPor             { get; set; } = string.Empty;
    public DateTime FechaSolicitud          { get; set; }
    public string Estado                    { get; set; } = string.Empty;
    public string? ResueltoPor              { get; set; }
    public DateTime? FechaResolucion        { get; set; }
    public string? MotivoRechazo            { get; set; }
}
