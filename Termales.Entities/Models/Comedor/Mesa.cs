using Termales.Entities.Enums;

namespace Termales.Entities.Models.Comedor;

public class Mesa
{
    public int MesaId { get; set; }
    public int Numero { get; set; }
    public int Capacidad { get; set; }
    public EstadoMesa Estado { get; set; } = EstadoMesa.Disponible;
    public bool Activo { get; set; } = true;

    // Cuando dos o más mesas se unen (grupo grande), las "secundarias"
    // apuntan a la mesa "principal" — la orden real vive en la principal,
    // las secundarias solo se muestran unidas visualmente en la grilla.
    public int? MesaPrincipalId { get; set; }
    public Mesa? MesaPrincipal { get; set; }
    public ICollection<Mesa> MesasSecundarias { get; set; } = new List<Mesa>();

    public ICollection<Orden> Ordenes { get; set; } = new List<Orden>();
}
