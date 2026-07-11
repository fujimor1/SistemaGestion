using Microsoft.EntityFrameworkCore;
using Termales.DAL.Context;
using Termales.DAL.Interfaces.Comedor;
using Termales.Entities.Models.Comedor;

namespace Termales.DAL.Repositories.Comedor;

public class ItemMenuRepository : GenericRepository<ItemMenu>, IItemMenuRepository
{
    public ItemMenuRepository(TermalesDbContext context) : base(context) { }

    public async Task<IEnumerable<ItemMenu>> ObtenerActivosAsync() =>
        await _dbSet
            .Include(i => i.Categoria)
            .Include(i => i.Receta).ThenInclude(r => r.Insumo)
            .Where(i => i.Activo)
            .OrderBy(i => i.Categoria.Nombre)
            .ThenBy(i => i.Nombre)
            .ToListAsync();

    public async Task<IEnumerable<ItemMenu>> ObtenerPorCategoriaAsync(int categoriaMenuId) =>
        await _dbSet
            .Include(i => i.Categoria)
            .Where(i => i.CategoriaMenuId == categoriaMenuId && i.Activo)
            .OrderBy(i => i.Nombre)
            .ToListAsync();

    public async Task<ItemMenu?> ObtenerConRecetaAsync(int id) =>
        await _dbSet
            .Include(i => i.Receta).ThenInclude(r => r.Insumo)
            .FirstOrDefaultAsync(i => i.ItemMenuId == id);
}
