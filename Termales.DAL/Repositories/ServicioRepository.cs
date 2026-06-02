using Microsoft.EntityFrameworkCore;
using Termales.DAL.Context;
using Termales.DAL.Interfaces;
using Termales.Entities.Models;

namespace Termales.DAL.Repositories;

public class ServicioRepository : GenericRepository<Servicio>, IServicioRepository
{
    public ServicioRepository(TermalesDbContext context) : base(context) { }

    public async Task<IEnumerable<Servicio>> ObtenerActivosAsync() =>
        await _dbSet.Where(s => s.Activo).OrderBy(s => s.Nombre).ToListAsync();
}
