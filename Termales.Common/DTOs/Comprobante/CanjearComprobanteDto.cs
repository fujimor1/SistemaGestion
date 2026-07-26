namespace Termales.Common.DTOs.Comprobante;

/// <summary>Canje de una Nota de Venta ya emitida por una Boleta o Factura — misma venta,
/// mismo monto y método de pago, solo cambia el tipo de documento.</summary>
public class CanjearComprobanteDto
{
    /// <summary>"BI" | "FI" — nunca "NV" ni "NC".</summary>
    public string TipoComprobante { get; set; } = string.Empty;

    // Cliente persona natural (Boleta)
    public string? ClienteDni    { get; set; }
    public string? ClienteNombre { get; set; }

    // Cliente empresa (Factura)
    public string? ClienteRuc         { get; set; }
    public string? ClienteRazonSocial { get; set; }
}
