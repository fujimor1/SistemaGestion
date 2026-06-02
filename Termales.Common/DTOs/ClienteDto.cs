using System.ComponentModel.DataAnnotations;

namespace Termales.Common.DTOs;

public class ClienteDto
{
    public int ClienteId { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string NombreCompleto => $"{Nombres} {Apellidos}";
    public string Dni { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Direccion { get; set; }
    public DateTime FechaRegistro { get; set; }
    public bool Activo { get; set; }
}

public class CrearClienteDto
{
    [Required(ErrorMessage = "Los nombres son requeridos")]
    [StringLength(100, MinimumLength = 2)]
    public string Nombres { get; set; } = string.Empty;

    [Required(ErrorMessage = "Los apellidos son requeridos")]
    [StringLength(100, MinimumLength = 2)]
    public string Apellidos { get; set; } = string.Empty;

    [Required(ErrorMessage = "El DNI es requerido")]
    [StringLength(8, MinimumLength = 8, ErrorMessage = "El DNI debe tener 8 dígitos")]
    [RegularExpression(@"^\d{8}$", ErrorMessage = "El DNI solo debe contener números")]
    public string Dni { get; set; } = string.Empty;

    [Phone]
    public string? Telefono { get; set; }

    [EmailAddress]
    public string? Email { get; set; }

    [StringLength(200)]
    public string? Direccion { get; set; }
}

public class ActualizarClienteDto : CrearClienteDto
{
    public int ClienteId { get; set; }
}
