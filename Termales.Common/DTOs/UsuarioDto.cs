using System.ComponentModel.DataAnnotations;

namespace Termales.Common.DTOs;

public class UsuarioDto
{
    public int UsuarioId { get; set; }
    public string Email { get; set; } = string.Empty;
    public int RolId { get; set; }
    public string NombreRol { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public int EmpleadoId { get; set; }
    public string NombreEmpleado { get; set; } = string.Empty;
}

public class CrearUsuarioDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
    public string Password { get; set; } = string.Empty;

    [Required]
    public int RolId { get; set; }

    /// <summary>Empleado a vincular con esta cuenta.</summary>
    [Required]
    public int EmpleadoId { get; set; }
}

public class ActualizarUsuarioDto
{
    public int UsuarioId { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public int RolId { get; set; }

    /// <summary>Empleado a vincular con esta cuenta.</summary>
    [Required]
    public int EmpleadoId { get; set; }
}

public class CambiarPasswordDto
{
    [Required]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
    public string NuevaPassword { get; set; } = string.Empty;
}

public class RolDto
{
    public int RolId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
}
