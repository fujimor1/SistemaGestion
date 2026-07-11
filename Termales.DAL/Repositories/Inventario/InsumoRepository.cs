using Microsoft.EntityFrameworkCore;
using Termales.DAL.Context;
using Termales.DAL.Interfaces.Inventario;
using Termales.Entities.Models.Inventario;

namespace Termales.DAL.Repositories.Inventario;

public class InsumoRepository : GenericRepository<Insumo>, IInsumoRepository
{
    public InsumoRepository(TermalesDbContext context) : base(context) { }

    public async Task<IEnumerable<Insumo>> ObtenerPorAmbienteAsync(string tipoAmbiente) =>
        await _dbSet
            .Where(i => i.TipoAmbiente == tipoAmbiente && i.Activo)
            .OrderBy(i => i.TipoArticulo).ThenBy(i => i.Nombre)
            .ToListAsync();

    public async Task<IEnumerable<Insumo>> ObtenerConEntradasAsync(string tipoAmbiente) =>
        await _dbSet
            .Include(i => i.Entradas.OrderByDescending(e => e.Fecha).Take(5))
            .Where(i => i.TipoAmbiente == tipoAmbiente && i.Activo)
            .OrderBy(i => i.TipoArticulo).ThenBy(i => i.Nombre)
            .ToListAsync();
}
