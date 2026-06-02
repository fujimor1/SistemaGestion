using Microsoft.EntityFrameworkCore;
using Termales.DAL.Context;
using Termales.DAL.Interfaces;
using Termales.Entities.Models;

namespace Termales.DAL.Repositories;

public class AforoRepository : GenericRepository<Aforo>, IAforoRepository
{
    public AforoRepository(TermalesDbContext context) : base(context) { }

    public async Task<Aforo?> ObtenerPorTipoYFechaAsync(int tipoServicioId, DateTime fecha) =>
        await _dbSet
            .Include(a => a.TipoServicio)
            .FirstOrDefaultAsync(a => a.TipoServicioId == tipoServicioId && a.Fecha.Date == fecha.Date);

    public async Task<IEnumerable<Aforo>> ObtenerPorFechaAsync(DateTime fecha) =>
        await _dbSet
            .Include(a => a.TipoServicio)
            .Where(a => a.Fecha.Date == fecha.Date)
            .ToListAsync();
}
