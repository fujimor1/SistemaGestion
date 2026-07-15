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

    /// <summary>Solo si MetodoPago == Mixto: cuánto de ese pago fue en efectivo (el resto, hasta el
    /// total, se asume Yape/Plin).</summary>
    public decimal? MontoEfectivoMixto { get; set; }
}

public class GenerarComprobanteComedorDto : GenerarComprobanteDto
{
    /// <summary>Líneas de la orden a cobrar en este comprobante (permite cobro parcial por grupo).</summary>
    public List<int> OrdenDetalleIds { get; set; } = new();
}

public class ItemBanioDto
{
    /// <summary>Exactamente uno de los dos: un servicio individual (Poza, Piscina) o un combo/paquete.</summary>
    public int? TipoServicioId { get; set; }
    public int? PaqueteBanioId { get; set; }
    public int Cantidad { get; set; } = 1;
}

public class GenerarComprobanteBanioDto : GenerarComprobanteDto
{
    /// <summary>Carrito de la venta: cada línea es un servicio individual o un combo, con su propia
    /// cantidad de personas — permite mezclar, ej. 1 persona en Poza y 1 en Piscina en la misma venta.</summary>
    public List<ItemBanioDto> Items { get; set; } = new();
}
