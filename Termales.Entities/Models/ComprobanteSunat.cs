using Termales.Entities.Enums;

namespace Termales.Entities.Models;

/// <summary>Detalle técnico del envío a SUNAT de un Comprobante (1:1). Separado de Comprobante porque
/// son datos pesados/de otro dominio (XML, CDR) que no se consultan junto al resto del POS.</summary>
public class ComprobanteSunat
{
    public int ComprobanteId { get; set; } // PK = FK a comprobantes.comprobante_id
    public Comprobante Comprobante { get; set; } = null!;

    public string XmlFirmado { get; set; } = string.Empty;
    public string? HashDigestValue { get; set; }

    public string? CdrXml { get; set; }
    public int? CdrCodigoRespuesta { get; set; }
    public string? CdrDescripcion { get; set; }
    public string? ObservacionesSunat { get; set; }

    public EstadoEnvioSunat Estado { get; set; } = EstadoEnvioSunat.Pendiente;
    public int IntentosEnvio { get; set; } = 0;

    public DateTime FechaLimiteEnvio { get; set; }
    public DateTime? FechaEnvioSunat { get; set; }

    // Sin uso hasta que Boleta (resumen diario) se migre en una fase posterior — se agrega ya la
    // columna para no tener que volver a migrar la tabla en ese momento.
    public string? TicketResumen { get; set; }
}
