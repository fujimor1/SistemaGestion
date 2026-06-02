using Termales.Entities.Models;

namespace Termales.DAL.Interfaces;

public interface ITipoServicioRepository : IGenericRepository<TipoServicio>
{
    Task<IEnumerable<TipoServicio>> ObtenerActivosAsync();
}
