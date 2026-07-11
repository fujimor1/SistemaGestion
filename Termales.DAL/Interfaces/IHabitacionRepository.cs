using Termales.Entities.Models;

namespace Termales.DAL.Interfaces;

public interface IHabitacionRepository : IGenericRepository<Habitacion>
{
    Task<IEnumerable<Habitacion>> ObtenerActivasAsync();
}
