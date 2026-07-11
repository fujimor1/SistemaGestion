using System.ComponentModel.DataAnnotations;

namespace Termales.Common.DTOs;

public class PaqueteBanioDto
{
    public int PaqueteBanioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public bool Activo { get; set; }
    public List<int> TipoServicioIds { get; set; } = new();
}

public class CrearPaqueteBanioDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    public decimal Precio { get; set; }

    [Required, MinLength(2, ErrorMessage = "Un paquete debe combinar al menos 2 tipos de servicio")]
    public List<int> TipoServicioIds { get; set; } = new();
}

public class ActualizarPaqueteBanioDto
{
    public int PaqueteBanioId { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    public decimal Precio { get; set; }

    [Required, MinLength(2, ErrorMessage = "Un paquete debe combinar al menos 2 tipos de servicio")]
    public List<int> TipoServicioIds { get; set; } = new();
}
