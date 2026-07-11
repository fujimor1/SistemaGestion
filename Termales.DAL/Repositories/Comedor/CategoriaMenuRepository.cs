using Microsoft.EntityFrameworkCore;
using Termales.DAL.Context;
using Termales.DAL.Interfaces.Comedor;
using Termales.Entities.Models.Comedor;

namespace Termales.DAL.Repositories.Comedor;

public class CategoriaMenuRepository : GenericRepository<CategoriaMenu>, ICategoriaMenuRepository
{
    public CategoriaMenuRepository(TermalesDbContext context) : base(context) { }

    public async Task<IEnumerable<CategoriaMenu>> ObtenerActivasAsync() =>
        await _dbSet.Where(c => c.Activo).OrderBy(c => c.Nombre).ToListAsync();
}
