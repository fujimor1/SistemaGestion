using Termales.Entities.Models;

namespace Termales.DAL.Interfaces;

public interface IPiscinaRepository : IGenericRepository<Piscina>
{
    Task<IEnumerable<Piscina>> ObtenerDisponiblesAsync();
    Task<IEnumerable<Piscina>> ObtenerDisponiblesEnFechaAsync(DateTime ingreso, DateTime salida);
}
