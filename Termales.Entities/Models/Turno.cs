using Termales.Entities.Enums;
using Termales.Entities.Models.Seguridad;

namespace Termales.Entities.Models;

public class Turno
{
    public int TurnoId { get; set; }
    public int TipoServicioId { get; set; }
    public DateTime FechaHora { get; set; }
    public int CantidadPersonas { get; set; }
    public decimal MontoTotal { get; set; }
    public EstadoPago EstadoPago { get; set; } = EstadoPago.Pendiente;
    public MetodoPago MetodoPago { get; set; }
    public int UsuarioId { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public TipoServicio TipoServicio { get; set; } = null!;
    public Usuario Usuario { get; set; } = null!;
}
