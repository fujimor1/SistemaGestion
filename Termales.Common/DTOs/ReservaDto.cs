using System.ComponentModel.DataAnnotations;
using Termales.Entities.Enums;

namespace Termales.Common.DTOs;

public class ReservaDto
{
    public int ReservaId { get; set; }
    public int ClienteId { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public int PiscinaId { get; set; }
    public string NombrePiscina { get; set; } = string.Empty;
    public DateTime FechaReserva { get; set; }
    public DateTime FechaIngreso { get; set; }
    public DateTime FechaSalida { get; set; }
    public int NumeroPersonas { get; set; }
    public decimal MontoTotal { get; set; }
    public EstadoReserva Estado { get; set; }
    public string EstadoDescripcion => Estado.ToString();
    public string? Observaciones { get; set; }
    public List<ReservaServicioDto> Servicios { get; set; } = new();
}

public class ReservaServicioDto
{
    public int ServicioId { get; set; }
    public string NombreServicio { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal => Cantidad * PrecioUnitario;
}

public class CrearReservaDto
{
    [Required]
    public int ClienteId { get; set; }

    [Required]
    public int PiscinaId { get; set; }

    [Required]
    public DateTime FechaIngreso { get; set; }

    [Required]
    public DateTime FechaSalida { get; set; }

    [Range(1, 200)]
    public int NumeroPersonas { get; set; }

    [StringLength(500)]
    public string? Observaciones { get; set; }

    public List<ServicioReservaDto> Servicios { get; set; } = new();
}

public class ServicioReservaDto
{
    public int ServicioId { get; set; }
    [Range(1, 100)]
    public int Cantidad { get; set; } = 1;
}

public class ActualizarEstadoReservaDto
{
    [Required]
    public EstadoReserva NuevoEstado { get; set; }
    public string? Observaciones { get; set; }
}
