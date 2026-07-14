using Termales.Entities.Models;

namespace Termales.DAL.Interfaces;

public interface IComprobanteSunatRepository : IGenericRepository<ComprobanteSunat>
{
    Task<ComprobanteSunat?> ObtenerPorComprobanteIdAsync(int comprobanteId);
    Task<IEnumerable<ComprobanteSunat>> ObtenerPendientesAsync();
}
