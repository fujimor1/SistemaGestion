using Termales.Entities.Models.Compras;
using Termales.Entities.Models.Tienda;

namespace Termales.Entities.Models.Inventario;

public class EntradaProducto
{
    public int EntradaProductoId { get; set; }
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Total { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public string? Observacion { get; set; }
    public int? CompraId { get; set; }

    public Producto Producto { get; set; } = null!;
    public Compra? Compra { get; set; }
}
