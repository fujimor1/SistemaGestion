namespace Termales.Common.DTOs.Comprobante;

public class ItemReciboDto
{
    public string  Descripcion    { get; set; } = string.Empty;
    public decimal Cantidad       { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Total          { get; set; }
}
