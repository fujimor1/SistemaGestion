using System.ComponentModel.DataAnnotations;

namespace Termales.Common.DTOs;

public class EmpleadoDto
{
    public int EmpleadoId { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string NombreCompleto => $"{Nombres} {Apellidos}";
    public string Dni { get; set; } = string.Empty;
    public bool Activo { get; set; }

    /// <summary>Indica si este empleado ya tiene una cuenta de usuario del sistema vinculada.</summary>
    public bool TieneUsuario { get; set; }
}

public class CrearEmpleadoDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Nombres { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Apellidos { get; set; } = string.Empty;

    [Required]
    [StringLength(8, MinimumLength = 8, ErrorMessage = "El DNI debe tener 8 dígitos")]
    [RegularExpression(@"^\d{8}$", ErrorMessage = "El DNI solo debe contener números")]
    public string Dni { get; set; } = string.Empty;
}

public class ActualizarEmpleadoDto
{
    public int EmpleadoId { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Nombres { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Apellidos { get; set; } = string.Empty;

    [Required]
    [StringLength(8, MinimumLength = 8, ErrorMessage = "El DNI debe tener 8 dígitos")]
    [RegularExpression(@"^\d{8}$", ErrorMessage = "El DNI solo debe contener números")]
    public string Dni { get; set; } = string.Empty;
}
