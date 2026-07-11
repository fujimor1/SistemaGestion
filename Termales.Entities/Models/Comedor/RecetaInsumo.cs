using Termales.Entities.Models.Inventario;

namespace Termales.Entities.Models.Comedor;

/// <summary>
/// Línea de receta: cuánto insumo consume UNA unidad de un plato del menú.
/// Cantidad se guarda en gramos cuando el insumo se mide en "kg" (el caso
/// más común); para cualquier otra unidad del insumo (litros, unidad, etc.)
/// se guarda directamente en esa misma unidad — ver ConversionUnidades.
/// </summary>
public class RecetaInsumo
{
    public int RecetaInsumoId { get; set; }
    public int ItemMenuId { get; set; }
    public int InsumoId { get; set; }
    public decimal Cantidad { get; set; }

    public ItemMenu ItemMenu { get; set; } = null!;
    public Insumo Insumo { get; set; } = null!;
}
