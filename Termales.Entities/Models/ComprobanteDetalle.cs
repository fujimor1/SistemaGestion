namespace Termales.Entities.Models;

public class ComprobanteDetalle
{
    public int ComprobanteDetalleId { get; set; }
    public int ComprobanteId { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }

    public Comprobante Comprobante { get; set; } = null!;
}
