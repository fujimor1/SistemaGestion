namespace Termales.Entities.Models;

public class ReservaServicio
{
    public int ReservaServicioId { get; set; }
    public int ReservaId { get; set; }
    public int ServicioId { get; set; }
    public int Cantidad { get; set; } = 1;
    public decimal PrecioUnitario { get; set; }

    public Reserva Reserva { get; set; } = null!;
    public Servicio Servicio { get; set; } = null!;
}
