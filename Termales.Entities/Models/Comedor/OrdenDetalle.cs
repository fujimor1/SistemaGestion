using Termales.Entities.Enums;
using Termales.Entities.Models.Tienda;
using Comprobante = Termales.Entities.Models.Comprobante;

namespace Termales.Entities.Models.Comedor;

public class OrdenDetalle
{
    public int OrdenDetalleId { get; set; }
    public int OrdenId { get; set; }

    // Exactamente uno de los dos: un plato de cocina (con receta e insumos)
    // o un producto de tienda (gaseosas, snacks, etc. — descuenta stock
    // directo, sin receta ni ticket de cocina).
    public int? ItemMenuId { get; set; }
    public int? ProductoId { get; set; }

    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal => Cantidad * PrecioUnitario;
    public EstadoOrdenDetalle Estado { get; set; } = EstadoOrdenDetalle.Pendiente;
    public string? Observaciones { get; set; }

    /// <summary>Comprobante que cobró esta línea — null mientras no se ha cobrado (cobro parcial por línea).</summary>
    public int? ComprobanteId { get; set; }

    public Orden Orden { get; set; } = null!;
    public ItemMenu? ItemMenu { get; set; }
    public Producto? Producto { get; set; }
    public Comprobante? Comprobante { get; set; }
}
