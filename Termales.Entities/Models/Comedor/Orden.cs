using Termales.Entities.Enums;
using Termales.Entities.Models.Seguridad;

namespace Termales.Entities.Models.Comedor;

public class Orden
{
    public int OrdenId { get; set; }
    public int MesaId { get; set; }
    public int UsuarioId { get; set; }
    public EstadoOrden Estado { get; set; } = EstadoOrden.Abierta;
    public decimal Total { get; set; }
    public string? Observaciones { get; set; }
    public string? MotivoCancelacion { get; set; }
    public DateTime FechaApertura { get; set; } = DateTime.UtcNow;
    public DateTime? FechaCierre { get; set; }

    public Mesa Mesa { get; set; } = null!;
    public Usuario Usuario { get; set; } = null!;
    public ICollection<OrdenDetalle> Detalles { get; set; } = new List<OrdenDetalle>();
}
