using Microsoft.EntityFrameworkCore;
using Termales.DAL.Context;
using Termales.DAL.Interfaces.Inventario;
using Termales.Entities.Models.Inventario;

namespace Termales.DAL.Repositories.Inventario;

public class SalidaInsumoRepository : GenericRepository<SalidaInsumo>, ISalidaInsumoRepository
{
    public SalidaInsumoRepository(TermalesDbContext context) : base(context) { }

    public async Task<IEnumerable<SalidaInsumo>> ObtenerPorInsumoAsync(int insumoId) =>
        await _dbSet
            .Where(s => s.InsumoId == insumoId)
            .OrderByDescending(s => s.Fecha)
            .ToListAsync();

    public async Task<IEnumerable<SalidaInsumo>> ObtenerPorFechaAsync(DateTime fecha) =>
        await _dbSet
            .Include(s => s.Insumo)
            .Where(s => s.Fecha.Date == fecha.Date)
            .OrderBy(s => s.Insumo.TipoAmbiente)
            .ThenBy(s => s.Insumo.Nombre)
            .ToListAsync();
}
