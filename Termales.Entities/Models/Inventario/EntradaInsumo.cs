using Termales.Entities.Models.Compras;

namespace Termales.Entities.Models.Inventario;

public class EntradaInsumo
{
    public int EntradaInsumoId { get; set; }
    public int InsumoId { get; set; }
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Total { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public string? Observacion { get; set; }
    public int? CompraId { get; set; }

    public Insumo Insumo { get; set; } = null!;
    public Compra? Compra { get; set; }
}
