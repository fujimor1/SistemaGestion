using Termales.Entities.Enums;

namespace Termales.Common.DTOs.Comprobante;

public class ComprobanteListadoDto
{
    public int ComprobanteId { get; set; }
    public string TipoComprobante { get; set; } = string.Empty;
    public string Serie { get; set; } = string.Empty;
    public int Numero { get; set; }
    public string NumeroFormateado => $"{Serie}-{Numero:D8}";
    public string TipoAmbiente { get; set; } = string.Empty;
    public string? ClienteNombre { get; set; }
    public string? Cajero { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaEmision { get; set; }
    public MetodoPago MetodoPago { get; set; }
    public bool Cobrado { get; set; }
    public DateTime? FechaCobro { get; set; }
    public int? ClienteId { get; set; }
}

public class MarcarCobradoDto
{
    public MetodoPago MetodoPago { get; set; }
}

/// <summary>Comprobante ya emitido, con sus ítems, para reimprimir/ver el ticket
/// desde "Comprobantes Emitidos" (el eventual PDF de Nubefact usa EnlacePdf en
/// su lugar; esto cubre las Notas de Venta y el modo simulación).</summary>
public class ComprobanteDetalleCompletoDto
{
    public int ComprobanteId { get; set; }
    public string TipoComprobante { get; set; } = string.Empty;
    public string Serie { get; set; } = string.Empty;
    public int Numero { get; set; }
    public string NumeroFormateado => $"{Serie}-{Numero:D8}";
    public string TipoAmbiente { get; set; } = string.Empty;
    public string? ClienteDni { get; set; }
    public string? ClienteRuc { get; set; }
    public string? ClienteNombre { get; set; }
    public string? ClienteRazonSocial { get; set; }
    public string? Cajero { get; set; }
    public decimal TotalGravada { get; set; }
    public decimal Impuesto { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? EnlacePdf { get; set; }
    public DateTime FechaEmision { get; set; }
    public List<ItemReciboDto> Items { get; set; } = new();
}
