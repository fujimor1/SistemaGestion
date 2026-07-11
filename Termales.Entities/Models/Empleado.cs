namespace Termales.Entities.Models;

public class Empleado
{
    public int EmpleadoId { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Dni { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}
