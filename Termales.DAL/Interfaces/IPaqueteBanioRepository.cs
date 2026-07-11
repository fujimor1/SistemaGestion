using Termales.Entities.Models;

namespace Termales.DAL.Interfaces;

public interface IPaqueteBanioRepository : IGenericRepository<PaqueteBanio>
{
    Task<IEnumerable<PaqueteBanio>> ObtenerActivosConTiposAsync();
    Task<PaqueteBanio?> ObtenerConTiposAsync(int id);
}
