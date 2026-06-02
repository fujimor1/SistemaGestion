namespace Termales.Entities.Models;

public class Piscina
{
    public int PiscinaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal TemperaturaGrados { get; set; }
    public int CapacidadPersonas { get; set; }
    public decimal TarifaPorHora { get; set; }
    public bool Disponible { get; set; } = true;

    public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}
