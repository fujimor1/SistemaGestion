using Termales.Entities.Enums;

namespace Termales.Entities.Models;

public class Pago
{
    public int PagoId { get; set; }
    public int ReservaId { get; set; }
    public decimal Monto { get; set; }
    public TipoPago TipoPago { get; set; }
    public DateTime FechaPago { get; set; } = DateTime.UtcNow;
    public string? NumeroComprobante { get; set; }
    public string? Observaciones { get; set; }

    public Reserva Reserva { get; set; } = null!;
}
