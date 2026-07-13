namespace Termales.Entities.Models;

// Lista de equipamiento fijo que tiene una habitación (TV, cama, colchón,
// etc.) — es solo un inventario de referencia, no se descuenta ni se
// consume como los insumos.
public class HabitacionItem
{
    public int HabitacionItemId { get; set; }
    public int HabitacionId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int Cantidad { get; set; } = 1;

    public Habitacion Habitacion { get; set; } = null!;
}
