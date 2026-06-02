using Microsoft.EntityFrameworkCore;
using Termales.DAL.Context;
using Termales.DAL.Interfaces;
using Termales.Entities.Models;

namespace Termales.DAL.Repositories;

public class TipoServicioRepository : GenericRepository<TipoServicio>, ITipoServicioRepository
{
    public TipoServicioRepository(TermalesDbContext context) : base(context) { }

    public async Task<IEnumerable<TipoServicio>> ObtenerActivosAsync() =>
        await _dbSet.Where(t => t.Activo).ToListAsync();
}
