namespace Termales.Entities.Models;

public class PaqueteBanioTipoServicio
{
    public int PaqueteBanioId { get; set; }
    public int TipoServicioId { get; set; }

    public PaqueteBanio PaqueteBanio { get; set; } = null!;
    public TipoServicio TipoServicio { get; set; } = null!;
}
