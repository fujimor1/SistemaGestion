namespace Termales.Entities.Models;

public class PaqueteBanio
{
    public int PaqueteBanioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public bool Activo { get; set; } = true;

    public ICollection<PaqueteBanioTipoServicio> Tipos { get; set; } = new List<PaqueteBanioTipoServicio>();
}
