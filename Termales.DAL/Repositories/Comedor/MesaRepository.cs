using Microsoft.EntityFrameworkCore;
using Termales.DAL.Context;
using Termales.DAL.Interfaces.Comedor;
using Termales.Entities.Enums;
using Termales.Entities.Models.Comedor;

namespace Termales.DAL.Repositories.Comedor;

public class MesaRepository : GenericRepository<Mesa>, IMesaRepository
{
    public MesaRepository(TermalesDbContext context) : base(context) { }

    public async Task<IEnumerable<Mesa>> ObtenerActivasAsync() =>
        await _dbSet.Where(m => m.Activo).OrderBy(m => m.Numero).ToListAsync();

    public async Task<Mesa?> ObtenerConOrdenActivaAsync(int mesaId) =>
        await _dbSet
            .Include(m => m.Ordenes.Where(o =>
                o.Estado != EstadoOrden.Pagada && o.Estado != EstadoOrden.Cancelada))
            .ThenInclude(o => o.Detalles)
            .ThenInclude(d => d.ItemMenu)
            .FirstOrDefaultAsync(m => m.MesaId == mesaId);
}
