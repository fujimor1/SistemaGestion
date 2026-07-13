using Microsoft.EntityFrameworkCore;
using Termales.DAL.Context;
using Termales.DAL.Interfaces;
using Termales.Entities.Models;

namespace Termales.DAL.Repositories;

public class HabitacionItemRepository : GenericRepository<HabitacionItem>, IHabitacionItemRepository
{
    public HabitacionItemRepository(TermalesDbContext context) : base(context) { }

    public async Task<IEnumerable<HabitacionItem>> ObtenerPorHabitacionAsync(int habitacionId) =>
        await _dbSet
            .Where(i => i.HabitacionId == habitacionId)
            .OrderBy(i => i.Nombre)
            .ToListAsync();
}
