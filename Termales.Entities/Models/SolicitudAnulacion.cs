namespace Termales.Entities.Models;

public class SolicitudAnulacion
{
    public int SolicitudAnulacionId { get; set; }

    public int         ComprobanteId              { get; set; }
    public Comprobante Comprobante                { get; set; } = null!;

    public string  Motivo                         { get; set; } = string.Empty;
    public string  SolicitadoPor                  { get; set; } = string.Empty;
    public string  EstadoAnteriorComprobante      { get; set; } = string.Empty;
    public DateTime FechaSolicitud                { get; set; } = DateTime.UtcNow;

    // "Pendiente" | "Aprobada" | "Rechazada"
    public string  Estado                         { get; set; } = "Pendiente";

    public string?  ResueltoPor                  { get; set; }
    public DateTime? FechaResolucion             { get; set; }
    public string?  MotivoRechazo               { get; set; }
}
