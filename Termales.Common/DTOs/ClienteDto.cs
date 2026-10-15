using System.ComponentModel.DataAnnotations;

namespace Termales.Common.DTOs;

public class ClienteDto
{
    public int ClienteId { get; set; }
    public string TipoDocumento { get; set; } = "DNI";
    public string NumeroDocumento { get; set; } = string.Empty;
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string? RazonSocial { get; set; }
    public string NombreCompleto => TipoDocumento == "RUC"
        ? (RazonSocial ?? NumeroDocumento)
        : $"{Nombres} {Apellidos}".Trim();
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Direccion { get; set; }
    public DateTime FechaRegistro { get; set; }
    public bool Activo { get; set; }
}

public class CrearClienteDto
{
    [Required(ErrorMessage = "El tipo de documento es requerido")]
    public string TipoDocumento { get; set; } = "DNI";

    [Required(ErrorMessage = "El número de documento es requerido")]
    public string NumeroDocumento { get; set; } = string.Empty;

    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string? RazonSocial { get; set; }

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
