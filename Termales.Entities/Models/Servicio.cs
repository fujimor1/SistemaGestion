namespace Termales.Entities.Models;

public class Servicio
{
    public int ServicioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }
    public bool Activo { get; set; } = true;

    public ICollection<ReservaServicio> ReservaServicios { get; set; } = new List<ReservaServicio>();
}
