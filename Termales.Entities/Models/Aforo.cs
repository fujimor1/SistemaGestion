namespace Termales.Entities.Models;

public class Aforo
{
    public int AforoId { get; set; }
    public int TipoServicioId { get; set; }
    public DateTime Fecha { get; set; }
    public int CapacidadMaxima { get; set; }
    public int OcupacionActual { get; set; }

    public int LugaresDisponibles => CapacidadMaxima - OcupacionActual;

    public TipoServicio TipoServicio { get; set; } = null!;
}
