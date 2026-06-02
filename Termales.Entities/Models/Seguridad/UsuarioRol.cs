namespace Termales.Entities.Models.Seguridad;

public class UsuarioRol
{
    public int UsuarioRolId { get; set; }
    public int UsuarioId { get; set; }
    public int RolId { get; set; }
    public DateTime FechaAsignacion { get; set; } = DateTime.UtcNow;

    public Usuario Usuario { get; set; } = null!;
    public Rol Rol { get; set; } = null!;
}
