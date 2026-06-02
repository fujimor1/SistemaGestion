namespace Termales.Entities.Models;

public class TipoServicio
{
    public int TipoServicioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int CapacidadMaxima { get; set; }
    public decimal PrecioPorPersona { get; set; }
    public bool Activo { get; set; } = true;

    public ICollection<Turno> Turnos { get; set; } = new List<Turno>();
    public ICollection<Aforo> Aforos { get; set; } = new List<Aforo>();
}
