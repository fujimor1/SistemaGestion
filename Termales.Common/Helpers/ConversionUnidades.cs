namespace Termales.Common.Helpers;

/// <summary>
/// Convierte cantidades entre la unidad en la que se define una receta y la
/// unidad en la que el inventario registra el stock del insumo.
/// Caso de uso concreto: las recetas se cargan en gramos (más preciso para
/// cocina), pero las entradas de inventario y el stock de insumos que se
/// pesan se registran en kilos — acá se hace ese 1000:1 automáticamente.
/// Para cualquier otra unidad del insumo (litros, unidad, etc.) no hay
/// conversión conocida: se asume que la receta ya está en esa misma unidad.
/// </summary>
public static class ConversionUnidades
{
    private static readonly HashSet<string> UnidadesEnKilos =
        new(StringComparer.OrdinalIgnoreCase) { "kg", "kilo", "kilos", "kilogramo", "kilogramos" };

    /// <summary>
    /// True si el insumo se mide en kilos (y por lo tanto su receta se
    /// carga en gramos).
    /// </summary>
    public static bool SeMideEnKilos(string? unidadInsumo) =>
        unidadInsumo is not null && UnidadesEnKilos.Contains(unidadInsumo.Trim());

    /// <summary>
    /// Convierte una cantidad de receta a la unidad en la que está el stock
    /// del insumo (kilos si el insumo se mide en kilos; si no, sin cambios).
    /// </summary>
    public static decimal RecetaAStockInsumo(decimal cantidadReceta, string? unidadInsumo) =>
        SeMideEnKilos(unidadInsumo) ? cantidadReceta / 1000m : cantidadReceta;
}
