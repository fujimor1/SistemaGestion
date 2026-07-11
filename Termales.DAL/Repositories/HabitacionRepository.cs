using Microsoft.EntityFrameworkCore;
using Termales.DAL.Context;
using Termales.DAL.Interfaces;
using Termales.Entities.Models;

namespace Termales.DAL.Repositories;

public class HabitacionRepository : GenericRepository<Habitacion>, IHabitacionRepository
{
    public HabitacionRepository(TermalesDbContext context) : base(context) { }

    public async Task<IEnumerable<Habitacion>> ObtenerActivasAsync() =>
        await _dbSet.Where(h => h.Activo).OrderBy(h => h.Nombre).ToListAsync();
}
