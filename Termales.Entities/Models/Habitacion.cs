namespace Termales.Entities.Models;

public class Habitacion
{
    public int HabitacionId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int Capacidad { get; set; }
    public decimal Precio { get; set; }
    public bool Ocupado { get; set; } = false;
    public bool Activo { get; set; } = true;

    /// <summary>Fecha/hora del check-in vigente (null si está libre).</summary>
    public DateTime? FechaCheckIn { get; set; }
    /// <summary>Fecha/hora del último check-out registrado.</summary>
    public DateTime? FechaCheckOut { get; set; }

    public ICollection<HabitacionItem> Items { get; set; } = new List<HabitacionItem>();
}
