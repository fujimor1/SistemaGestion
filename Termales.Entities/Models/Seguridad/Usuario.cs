using Termales.Entities.Models;

namespace Termales.Entities.Models.Seguridad;

public class Usuario
{
    public int UsuarioId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public int RolId { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    /// <summary>Empleado vinculado a esta cuenta — obligatorio, toda cuenta se crea a partir de un Empleado existente.</summary>
    public int EmpleadoId { get; set; }

    public Rol Rol { get; set; } = null!;
    public Empleado Empleado { get; set; } = null!;
    public ICollection<UsuarioRol> UsuarioRoles { get; set; } = new List<UsuarioRol>();
}
