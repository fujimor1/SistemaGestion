using System.ComponentModel.DataAnnotations;

namespace Termales.Common.DTOs;

public class PiscinaDto
{
    public int PiscinaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal TemperaturaGrados { get; set; }
    public int CapacidadPersonas { get; set; }
    public decimal TarifaPorHora { get; set; }
    public bool Disponible { get; set; }
}

public class CrearPiscinaDto
{
    [Required]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Descripcion { get; set; }

    [Range(20, 50, ErrorMessage = "La temperatura debe estar entre 20°C y 50°C")]
    public decimal TemperaturaGrados { get; set; }

    [Range(1, 200)]
    public int CapacidadPersonas { get; set; }

    [Range(0.01, 9999.99)]
    public decimal TarifaPorHora { get; set; }
}

public class ActualizarPiscinaDto : CrearPiscinaDto
{
    public int PiscinaId { get; set; }
    public bool Disponible { get; set; }
}
