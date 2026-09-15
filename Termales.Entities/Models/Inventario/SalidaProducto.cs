using Termales.Entities.Models.Tienda;

namespace Termales.Entities.Models.Inventario;

public class SalidaProducto
{
    public int SalidaProductoId { get; set; }
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public string? Observacion { get; set; }
    public Producto Producto { get; set; } = null!;
}
