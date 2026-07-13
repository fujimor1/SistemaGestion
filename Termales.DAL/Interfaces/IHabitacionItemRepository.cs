using Termales.Entities.Models;

namespace Termales.DAL.Interfaces;

public interface IHabitacionItemRepository : IGenericRepository<HabitacionItem>
{
    Task<IEnumerable<HabitacionItem>> ObtenerPorHabitacionAsync(int habitacionId);
}
