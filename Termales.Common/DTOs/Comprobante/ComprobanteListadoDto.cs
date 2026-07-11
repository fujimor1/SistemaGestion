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
