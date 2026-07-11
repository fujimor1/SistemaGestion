using Termales.Entities.Enums;

namespace Termales.Entities.Models.Comedor;

public class Mesa
{
    public int MesaId { get; set; }
    public int Numero { get; set; }
    public int Capacidad { get; set; }
    public EstadoMesa Estado { get; set; } = EstadoMesa.Disponible;
    public bool Activo { get; set; } = true;

    public ICollection<Orden> Ordenes { get; set; } = new List<Orden>();
}
