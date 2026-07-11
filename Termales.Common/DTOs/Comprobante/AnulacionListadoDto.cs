namespace Termales.Common.DTOs.Comprobante;

public class AnulacionListadoDto
{
    public int    ComprobanteId     { get; set; }
    public string TipoComprobante  { get; set; } = string.Empty;
    public string Serie             { get; set; } = string.Empty;
    public int    Numero            { get; set; }
    public string NumeroFormateado  { get; set; } = string.Empty;
    public string TipoAmbiente      { get; set; } = string.Empty;
    public string? ClienteNombre    { get; set; }
    public string? Cajero           { get; set; }
    public decimal Total            { get; set; }
    public string? MotivoAnulacion  { get; set; }
    public string? AutorizadoPor    { get; set; }
    public DateTime FechaEmision    { get; set; }
}
