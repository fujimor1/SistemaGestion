using Microsoft.EntityFrameworkCore;
using Termales.DAL.Context;
using Termales.DAL.Interfaces;
using Termales.Entities.Models;

namespace Termales.DAL.Repositories;

public class PaqueteBanioRepository : GenericRepository<PaqueteBanio>, IPaqueteBanioRepository
{
    public PaqueteBanioRepository(TermalesDbContext context) : base(context) { }

    public async Task<IEnumerable<PaqueteBanio>> ObtenerActivosConTiposAsync() =>
        await _dbSet.Include(p => p.Tipos).ThenInclude(t => t.TipoServicio)
            .Where(p => p.Activo)
            .ToListAsync();

    public async Task<PaqueteBanio?> ObtenerConTiposAsync(int id) =>
        await _dbSet.Include(p => p.Tipos).ThenInclude(t => t.TipoServicio)
            .FirstOrDefaultAsync(p => p.PaqueteBanioId == id);
}
