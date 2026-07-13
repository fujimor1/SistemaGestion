using System.ComponentModel.DataAnnotations;

namespace Termales.Common.DTOs;

public class HabitacionDto
{
    public int HabitacionId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int Capacidad { get; set; }
    public decimal Precio { get; set; }
    public bool Ocupado { get; set; }
    public bool Activo { get; set; }
    public DateTime? FechaCheckIn { get; set; }
    public DateTime? FechaCheckOut { get; set; }
}

public class CrearHabitacionDto
{
    [Required][StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Descripcion { get; set; }

    [Range(1, 20)]
    public int Capacidad { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
    public decimal Precio { get; set; }
}

public class ActualizarHabitacionDto : CrearHabitacionDto
{
    public int HabitacionId { get; set; }
}

public class HabitacionItemDto
{
    public int HabitacionItemId { get; set; }
    public int HabitacionId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int Cantidad { get; set; }
}

public class CrearHabitacionItemDto
{
    [Required][StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Range(1, 999)]
    public int Cantidad { get; set; } = 1;
}
