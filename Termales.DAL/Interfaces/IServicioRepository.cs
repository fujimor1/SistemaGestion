using Termales.Entities.Models;

namespace Termales.DAL.Interfaces;

public interface IServicioRepository : IGenericRepository<Servicio>
{
    Task<IEnumerable<Servicio>> ObtenerActivosAsync();
}
