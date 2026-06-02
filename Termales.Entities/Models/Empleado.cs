using Termales.Entities.Enums;

namespace Termales.Entities.Models;

public class Empleado
{
    public int EmpleadoId { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Dni { get; set; } = string.Empty;
    public CargoEmpleado Cargo { get; set; }
    public string? Telefono { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime FechaContrato { get; set; }
    public bool Activo { get; set; } = true;
}
