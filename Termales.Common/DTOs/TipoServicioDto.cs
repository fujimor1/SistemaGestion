using System.ComponentModel.DataAnnotations;

namespace Termales.Common.DTOs;

public class TipoServicioDto
{
    public int TipoServicioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int CapacidadMaxima { get; set; }
    public decimal PrecioPorPersona { get; set; }
    public bool Activo { get; set; }
}

public class CrearTipoServicioDto
{
    [Required]
    [StringLength(150, MinimumLength = 2)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Descripcion { get; set; }

    [Required, Range(1, 10000)]
    public int CapacidadMaxima { get; set; }

    [Required, Range(0.01, 99999)]
    public decimal PrecioPorPersona { get; set; }
}

public class ActualizarTipoServicioDto : CrearTipoServicioDto
{
    public int TipoServicioId { get; set; }
}
