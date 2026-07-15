namespace Termales.Common.DTOs.Comprobante;

/// <summary>Listado de Notas de Crédito (directas o por aprobación de anulación).</summary>
public class NotaCreditoListadoDto
{
    public int ComprobanteId { get; set; }
    public string NumeroFormateado { get; set; } = string.Empty;
    public string? ComprobanteOrigenTipo { get; set; } // BI | FI del comprobante que referencia
    public string? ComprobanteOrigenNumeroFormateado { get; set; }
    public string? Motivo { get; set; }
    public DateTime FechaEmision { get; set; }
    public decimal Total { get; set; }
    public string? Cajero { get; set; }
    public string Estado { get; set; } = string.Empty;
}
