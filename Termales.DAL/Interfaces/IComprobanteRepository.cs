using Termales.Entities.Models;

namespace Termales.DAL.Interfaces;

public interface IComprobanteRepository : IGenericRepository<Comprobante>
{
    Task<int> ObtenerUltimoNumeroAsync(string serie);
    Task<IEnumerable<Comprobante>> ObtenerPorFechaAsync(DateOnly fecha, string? tipoAmbiente);
    Task<IEnumerable<Comprobante>> ObtenerAnulacionesAsync(DateOnly? desde, DateOnly? hasta);
    Task<IEnumerable<Comprobante>> ObtenerPendientesDeCobroAsync();
    Task<Comprobante?> ObtenerConDetalleAsync(int comprobanteId);
    Task<IEnumerable<Comprobante>> ObtenerFacturasBoletasAsync(DateOnly fecha);
    Task<IEnumerable<Comprobante>> ObtenerNotasCreditoAsync(DateOnly desde, DateOnly hasta);
}
