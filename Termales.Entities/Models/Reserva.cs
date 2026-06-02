using Termales.Entities.Enums;

namespace Termales.Entities.Models;

public class Reserva
{
    public int ReservaId { get; set; }
    public int ClienteId { get; set; }
    public int PiscinaId { get; set; }
    public DateTime FechaReserva { get; set; }
    public DateTime FechaIngreso { get; set; }
    public DateTime FechaSalida { get; set; }
    public int NumeroPersonas { get; set; }
    public decimal MontoTotal { get; set; }
    public EstadoReserva Estado { get; set; } = EstadoReserva.Pendiente;
    public string? Observaciones { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public Cliente Cliente { get; set; } = null!;
    public Piscina Piscina { get; set; } = null!;
    public Pago? Pago { get; set; }
    public ICollection<ReservaServicio> ReservaServicios { get; set; } = new List<ReservaServicio>();
}
