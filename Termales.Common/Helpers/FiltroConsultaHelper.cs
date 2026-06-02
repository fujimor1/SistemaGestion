namespace Termales.Common.Helpers;

public class FiltroConsulta
{
    private const int MaxTamanoPagina = 50;
    private int _tamanoPagina = 10;

    public int Pagina { get; set; } = 1;
    public int TamanoPagina
    {
        get => _tamanoPagina;
        set => _tamanoPagina = value > MaxTamanoPagina ? MaxTamanoPagina : value;
    }
    public string? Busqueda { get; set; }
    public string? OrdenarPor { get; set; }
    public bool OrdenDescendente { get; set; }
}

public class FiltroReserva : FiltroConsulta
{
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
    public int? ClienteId { get; set; }
    public int? PiscinaId { get; set; }
    public string? Estado { get; set; }
}
