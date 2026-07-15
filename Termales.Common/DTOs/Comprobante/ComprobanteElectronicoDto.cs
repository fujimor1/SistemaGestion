namespace Termales.Common.DTOs.Comprobante;

/// <summary>Listado de Facturas/Boletas para el módulo de Facturación Electrónica.</summary>
public class ComprobanteElectronicoDto
{
    public int ComprobanteId { get; set; }
    public string NumeroFormateado { get; set; } = string.Empty;
    public string TipoComprobante { get; set; } = string.Empty; // BI | FI
    public string TipoAmbiente { get; set; } = string.Empty;
    public string? ClienteNombre { get; set; }
    public string? ClienteDocumento { get; set; } // RUC o DNI, según corresponda
    public string Detalle { get; set; } = string.Empty;
    public DateTime FechaEmision { get; set; }
    public decimal Total { get; set; }
    public string? Cajero { get; set; }
    public int MetodoPago { get; set; }
    public string Estado { get; set; } = string.Empty;
}
