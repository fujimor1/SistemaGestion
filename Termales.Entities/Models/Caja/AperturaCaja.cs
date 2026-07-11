namespace Termales.Entities.Models.Caja;

public class AperturaCaja
{
    public int AperturaCajaId { get; set; }
    public DateTime Fecha { get; set; }
    public decimal MontoInicial { get; set; }
    public string Responsable { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
}
