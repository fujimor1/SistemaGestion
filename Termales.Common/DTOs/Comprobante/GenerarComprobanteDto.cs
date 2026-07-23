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

public class ItemComedorCobroDto
{
    public int OrdenDetalleId { get; set; }

    /// <summary>Cuántas unidades de esa línea se cobran ahora — puede ser menor a la cantidad
    /// total de la línea (ej. cobrar 2 de 4 cafés), en cuyo caso la línea se divide: el resto
    /// queda pendiente para que el grupo lo cobre después.</summary>
    public int Cantidad { get; set; }
}

public class GenerarComprobanteComedorDto : GenerarComprobanteDto
{
    /// <summary>Líneas (y cuántas unidades de cada una) a cobrar en este comprobante — permite
    /// cobro parcial tanto por línea como dentro de una misma línea.</summary>
    public List<ItemComedorCobroDto> Items { get; set; } = new();
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
