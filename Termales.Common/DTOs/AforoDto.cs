using System.ComponentModel.DataAnnotations;

namespace Termales.Common.DTOs;

public class AforoDto
{
    public int AforoId { get; set; }
    public int TipoServicioId { get; set; }
    public string NombreTipoServicio { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public int CapacidadMaxima { get; set; }
    public int OcupacionActual { get; set; }
    public int LugaresDisponibles => CapacidadMaxima - OcupacionActual;
}

public class CrearAforoDto
{
    [Required]
    public int TipoServicioId { get; set; }

    [Required]
    public DateTime Fecha { get; set; }

    [Required, Range(1, 10000)]
    public int CapacidadMaxima { get; set; }
}

public class ActualizarAforoDto : CrearAforoDto
{
    public int AforoId { get; set; }
}
