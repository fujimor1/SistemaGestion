using Termales.Entities.Enums;

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

    /// <summary>Posición para el orden manual (drag&amp;drop) en la lista de habitaciones.</summary>
    public int Orden { get; set; }

    /// <summary>Al liberar (checkout) queda en PorLimpiar automáticamente — el personal de
    /// limpieza la marca como Limpia antes de poder asignarla a un cliente nuevo.</summary>
    public EstadoLimpieza EstadoLimpieza { get; set; } = EstadoLimpieza.Limpia;

    /// <summary>Fecha/hora del check-in vigente (null si está libre).</summary>
    public DateTime? FechaCheckIn { get; set; }
    /// <summary>Fecha/hora del último check-out registrado.</summary>
    public DateTime? FechaCheckOut { get; set; }

    public ICollection<HabitacionItem> Items { get; set; } = new List<HabitacionItem>();
}
