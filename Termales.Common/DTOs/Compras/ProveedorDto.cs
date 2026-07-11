using System.ComponentModel.DataAnnotations;

namespace Termales.Common.DTOs.Compras;

public class ProveedorDto
{
    public int ProveedorId { get; set; }
    public string Ruc { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string? NombreComercial { get; set; }
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public bool Activo { get; set; }
}

public class CrearProveedorDto
{
    [Required]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "El RUC debe tener 11 dígitos")]
    [RegularExpression(@"^\d{11}$", ErrorMessage = "El RUC solo debe contener números")]
    public string Ruc { get; set; } = string.Empty;

    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string RazonSocial { get; set; } = string.Empty;

    public string? NombreComercial { get; set; }
    public string? Direccion { get; set; }

    [Phone]
    public string? Telefono { get; set; }

    [EmailAddress]
    public string? Email { get; set; }
}

public class ActualizarProveedorDto
{
    public int ProveedorId { get; set; }

    [Required]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "El RUC debe tener 11 dígitos")]
    [RegularExpression(@"^\d{11}$", ErrorMessage = "El RUC solo debe contener números")]
    public string Ruc { get; set; } = string.Empty;

    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string RazonSocial { get; set; } = string.Empty;

    public string? NombreComercial { get; set; }
    public string? Direccion { get; set; }

    [Phone]
    public string? Telefono { get; set; }

    [EmailAddress]
    public string? Email { get; set; }
}
