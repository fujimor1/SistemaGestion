namespace Termales.Common.DTOs.Sunat;

public class ComprobanteSunatPendienteDto
{
    public int ComprobanteId { get; set; }
    public string Serie { get; set; } = string.Empty;
    public int Numero { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; } = string.Empty;
    public int IntentosEnvio { get; set; }
    public DateTime FechaLimiteEnvio { get; set; }
    public string? UltimoError { get; set; }
}
