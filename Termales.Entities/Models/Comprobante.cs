using Termales.Entities.Enums;

namespace Termales.Entities.Models;

public class Comprobante
{
    public int ComprobanteId { get; set; }

    // Identificación
    public string Serie           { get; set; } = string.Empty;
    public int    Numero          { get; set; }
    public string TipoComprobante { get; set; } = string.Empty; // NV | BI | FI
    public string Local           { get; set; } = "Local Principal";

    // Referencia interna
    public string TipoAmbiente { get; set; } = string.Empty; // comedor | banio | habitacion
    public int?   ReferenciaId { get; set; }

    // Cliente
    public string? ClienteDni        { get; set; }
    public string? ClienteRuc        { get; set; }
    public string? ClienteNombre     { get; set; }
    public string? ClienteRazonSocial { get; set; }

    // Emisor
    public string? Cajero { get; set; }

    // Montos
    public string  Moneda       { get; set; } = "PEN";
    public decimal TotalGravada { get; set; }
    public decimal Impuesto     { get; set; }
    public decimal Total        { get; set; }

    // Estado y enlace
    public string Estado    { get; set; } = string.Empty; // EMITIDO | SIMULADO | ENVIADO A SUNAT | ANULADO
    public string EnlacePdf { get; set; } = string.Empty;

    // Para Notas de Crédito: referencia al comprobante original
    public int? ComprobanteOrigenId { get; set; }
    public Comprobante? ComprobanteOrigen { get; set; }

    // Anulación: motivo y supervisor que autorizó
    public string? MotivoAnulacion { get; set; }
    public string? AutorizadoPor   { get; set; }

    // Nota de crédito directa a SUNAT: código de motivo (catálogo 09, 2 dígitos, ej. "01")
    public string? CodigoMotivoNc { get; set; }

    // Cobro: el comprobante se emite igual si es fiado (obligación SUNAT), pero
    // no cuenta como ingreso de caja hasta que Cobrado pase a true.
    public MetodoPago MetodoPago { get; set; } = MetodoPago.Efectivo;

    // Solo si MetodoPago == Mixto: cuánto de ese pago fue en efectivo (el resto es Yape/Plin).
    public decimal? MontoEfectivoMixto { get; set; }

    public bool Cobrado { get; set; } = true;
    public DateTime? FechaCobro { get; set; }
    public int? ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    public DateTime FechaEmision { get; set; } = DateTime.UtcNow;

    public ICollection<ComprobanteDetalle> Detalles { get; set; } = new List<ComprobanteDetalle>();
}
