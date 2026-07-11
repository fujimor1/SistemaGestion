using Termales.Entities.Models;

namespace Termales.DAL.Interfaces;

public interface IComprobanteRepository : IGenericRepository<Comprobante>
{
    Task<int> ObtenerUltimoNumeroAsync(string serie);
    Task<IEnumerable<Comprobante>> ObtenerPorFechaAsync(DateOnly fecha, string? tipoAmbiente);
    Task<IEnumerable<Comprobante>> ObtenerAnulacionesAsync(DateOnly? desde, DateOnly? hasta);
    Task<IEnumerable<Comprobante>> ObtenerPendientesDeCobroAsync();
}
