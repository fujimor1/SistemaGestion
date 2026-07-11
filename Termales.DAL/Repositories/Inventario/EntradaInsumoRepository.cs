using Microsoft.EntityFrameworkCore;
using Termales.DAL.Context;
using Termales.DAL.Interfaces.Inventario;
using Termales.Entities.Models.Inventario;

namespace Termales.DAL.Repositories.Inventario;

public class EntradaInsumoRepository : GenericRepository<EntradaInsumo>, IEntradaInsumoRepository
{
    public EntradaInsumoRepository(TermalesDbContext context) : base(context) { }

    public async Task<IEnumerable<EntradaInsumo>> ObtenerPorInsumoAsync(int insumoId) =>
        await _dbSet
            .Where(e => e.InsumoId == insumoId)
            .OrderByDescending(e => e.Fecha)
            .ToListAsync();
}
