using Termales.Entities.Models.Inventario;
using Termales.Entities.Models.Tienda;

namespace Termales.Entities.Models.Compras;

public class DetalleCompra
{
    public int DetalleCompraId { get; set; }
    public int CompraId { get; set; }

    public string TipoItem { get; set; } = string.Empty; // INSUMO | PRODUCTO
    public int? InsumoId { get; set; }
    public int? ProductoId { get; set; }

    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Total { get; set; }

    public Compra Compra { get; set; } = null!;
    public Insumo? Insumo { get; set; }
    public Producto? Producto { get; set; }
}
