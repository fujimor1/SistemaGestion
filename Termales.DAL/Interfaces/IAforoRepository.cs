using Termales.Entities.Models;

namespace Termales.DAL.Interfaces;

public interface IAforoRepository : IGenericRepository<Aforo>
{
    Task<Aforo?> ObtenerPorTipoYFechaAsync(int tipoServicioId, DateTime fecha);
    Task<IEnumerable<Aforo>> ObtenerPorFechaAsync(DateTime fecha);
}
