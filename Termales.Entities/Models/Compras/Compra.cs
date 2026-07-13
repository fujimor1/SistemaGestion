namespace Termales.Entities.Models.Compras;

public class Compra
{
    public int CompraId { get; set; }
    public int? ProveedorId { get; set; }
    public string? NombreProveedorManual { get; set; }

    public string TipoComprobante { get; set; } = string.Empty; // FACTURA | BOLETA | GUIA
    // Nulos cuando TipoComprobante es GUIA: hay guías de remisión que no
    // traen serie ni número.
    public string? Serie { get; set; }
    public int? Numero { get; set; }
    public DateTime FechaEmision { get; set; }

    public string FormaPago { get; set; } = string.Empty; // CONTADO | CREDITO
    public DateTime? FechaVencimiento { get; set; }

    public string Moneda { get; set; } = "PEN";
    public decimal TotalGravada { get; set; }
    public decimal Igv { get; set; }
    public decimal Total { get; set; }

    public string Estado { get; set; } = "REGISTRADA"; // REGISTRADA | PAGADA | ANULADA
    public string? Observaciones { get; set; }
    public string RegistradoPor { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    public DateTime? FechaPago { get; set; }
    public int? EgresoCajaChicaId { get; set; }

    public Proveedor? Proveedor { get; set; }
    public ICollection<DetalleCompra> Detalles { get; set; } = new List<DetalleCompra>();
}
