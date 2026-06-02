using Microsoft.EntityFrameworkCore;
using Termales.DAL.Context;
using Termales.DAL.Interfaces;
using Termales.Entities.Models;

namespace Termales.DAL.Repositories;

public class TurnoRepository : GenericRepository<Turno>, ITurnoRepository
{
    public TurnoRepository(TermalesDbContext context) : base(context) { }

    public async Task<Turno?> ObtenerConDetallesAsync(int turnoId) =>
        await _dbSet
            .Include(t => t.TipoServicio)
            .Include(t => t.Usuario)
            .FirstOrDefaultAsync(t => t.TurnoId == turnoId);

    public async Task<IEnumerable<Turno>> ObtenerPorTipoYFechaAsync(int tipoServicioId, DateTime fecha) =>
        await _dbSet
            .Include(t => t.TipoServicio)
            .Where(t => t.TipoServicioId == tipoServicioId && t.FechaHora.Date == fecha.Date)
            .OrderBy(t => t.FechaHora)
            .ToListAsync();
}
