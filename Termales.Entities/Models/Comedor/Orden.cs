using Termales.Entities.Enums;
using Termales.Entities.Models.Seguridad;

namespace Termales.Entities.Models.Comedor;

public class Orden
{
    public int OrdenId { get; set; }
    // Null cuando TipoEntrega es "llevar": un pedido para llevar no ocupa
    // ninguna mesa física.
    public int? MesaId { get; set; }
    public int UsuarioId { get; set; }
    public EstadoOrden Estado { get; set; } = EstadoOrden.Abierta;
    /// <summary>"comedor" (se sirve en una mesa) | "llevar" (para llevar, sin mesa).</summary>
    public string TipoEntrega { get; set; } = "comedor";
    public decimal Total { get; set; }
    public string? Observaciones { get; set; }
    public string? MotivoCancelacion { get; set; }
    public DateTime FechaApertura { get; set; } = DateTime.UtcNow;
    public DateTime? FechaCierre { get; set; }

    public Mesa? Mesa { get; set; }
    public Usuario Usuario { get; set; } = null!;
    public ICollection<OrdenDetalle> Detalles { get; set; } = new List<OrdenDetalle>();
}
