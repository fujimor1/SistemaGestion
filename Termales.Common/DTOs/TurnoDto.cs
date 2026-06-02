using System.ComponentModel.DataAnnotations;
using Termales.Entities.Enums;

namespace Termales.Common.DTOs;

public class TurnoDto
{
    public int TurnoId { get; set; }
    public int TipoServicioId { get; set; }
    public string NombreTipoServicio { get; set; } = string.Empty;
    public DateTime FechaHora { get; set; }
    public int CantidadPersonas { get; set; }
    public decimal MontoTotal { get; set; }
    public EstadoPago EstadoPago { get; set; }
    public string EstadoPagoDescripcion => EstadoPago.ToString();
    public MetodoPago MetodoPago { get; set; }
    public string MetodoPagoDescripcion => MetodoPago == MetodoPago.YapePlin ? "Yape/Plin" : MetodoPago.ToString();
    public int UsuarioId { get; set; }
    public DateTime FechaCreacion { get; set; }
}

public class RegistrarTurnoDto
{
    [Required]
    public int TipoServicioId { get; set; }

    [Required]
    public DateTime FechaHora { get; set; }

    [Required, Range(1, 200)]
    public int CantidadPersonas { get; set; }

    [Required]
    public MetodoPago MetodoPago { get; set; }

    [Required]
    public int UsuarioId { get; set; }
}

public class DisponibilidadDto
{
    public int TipoServicioId { get; set; }
    public string NombreTipoServicio { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public int CapacidadMaxima { get; set; }
    public int OcupacionActual { get; set; }
    public int LugaresDisponibles => CapacidadMaxima - OcupacionActual;
    public bool Disponible => LugaresDisponibles > 0;
}

public class TipoServicioDto
{
    public int TipoServicioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int CapacidadMaxima { get; set; }
    public decimal PrecioPorPersona { get; set; }
    public bool Activo { get; set; }
}
