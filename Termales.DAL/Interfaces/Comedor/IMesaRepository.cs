using Termales.Entities.Models.Comedor;

namespace Termales.DAL.Interfaces.Comedor;

public interface IMesaRepository : IGenericRepository<Mesa>
{
    Task<IEnumerable<Mesa>> ObtenerActivasAsync();
    Task<Mesa?> ObtenerConOrdenActivaAsync(int mesaId);
    Task<Mesa?> ObtenerConSecundariasAsync(int mesaId);
}
