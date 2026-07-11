namespace Termales.Common.DTOs.Comprobante;

public class ComprobanteResultadoDto
{
    public int    ComprobanteId    { get; set; }
    public string TipoComprobante  { get; set; } = string.Empty; // NV | BI | FI
    public string Serie            { get; set; } = string.Empty;
    public int    Numero           { get; set; }
    public string NumeroFormateado { get; set; } = string.Empty; // ej. NV01-00001
    public string Ambiente         { get; set; } = string.Empty; // comedor | banio | habitacion | tienda
    public string Local            { get; set; } = "Local Principal";
    public string? Cajero          { get; set; }
    public string Moneda           { get; set; } = "PEN";
    public decimal TotalGravada    { get; set; }
    public decimal Impuesto        { get; set; }
    public decimal Total           { get; set; }
    public string Estado           { get; set; } = string.Empty; // EMITIDO | SIMULADO | ENVIADO A SUNAT
    public string EnlacePdf        { get; set; } = string.Empty;
    public bool   ModoSimulacion   { get; set; }
}
