using Termales.Entities.Enums;

namespace Termales.Common.DTOs.Comprobante;

public class GenerarComprobanteDto
{
    /// <summary>NV = Nota de Venta | BI = Boleta | FI = Factura</summary>
    public string TipoComprobante { get; set; } = "NV";

    // Cliente persona natural (NV / Boleta)
    public string? ClienteDni    { get; set; }
    public string? ClienteNombre { get; set; }

    // Cliente empresa (Factura)
    public string? ClienteRuc          { get; set; }
    public string? ClienteRazonSocial  { get; set; }

    /// <summary>Requerido para baño/habitación. Para comedor se toma del total de la orden.</summary>
    public decimal? Monto { get; set; }

    public MetodoPago MetodoPago { get; set; } = MetodoPago.Efectivo;

    /// <summary>Cliente registrado a vincular con la deuda, solo relevante si MetodoPago == Fiado.</summary>
    public int? ClienteId { get; set; }
}

public class GenerarComprobanteComedorDto : GenerarComprobanteDto
{
    /// <summary>Líneas de la orden a cobrar en este comprobante (permite cobro parcial por grupo).</summary>
    public List<int> OrdenDetalleIds { get; set; } = new();
}

public class GenerarComprobanteBanioDto : GenerarComprobanteDto
{
    /// <summary>Servicios elegidos (ej. Poza, Piscina) — el precio sale de la lista de precios, no se teclea.</summary>
    public List<int> TipoServicioIds { get; set; } = new();
    public int CantidadPersonas { get; set; } = 1;
}
