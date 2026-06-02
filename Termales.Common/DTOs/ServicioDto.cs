using System.ComponentModel.DataAnnotations;

namespace Termales.Common.DTOs;

public class ServicioDto
{
    public int ServicioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }
    public bool Activo { get; set; }
}

public class CrearServicioDto
{
    [Required]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Descripcion { get; set; }

    [Range(0.01, 9999.99)]
    public decimal Precio { get; set; }
}

public class ActualizarServicioDto : CrearServicioDto
{
    public int ServicioId { get; set; }
    public bool Activo { get; set; }
}
