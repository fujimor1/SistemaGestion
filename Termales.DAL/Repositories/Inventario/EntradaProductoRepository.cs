using Microsoft.EntityFrameworkCore;
using Termales.DAL.Context;
using Termales.DAL.Interfaces.Inventario;
using Termales.Entities.Models.Inventario;

namespace Termales.DAL.Repositories.Inventario;

public class EntradaProductoRepository : GenericRepository<EntradaProducto>, IEntradaProductoRepository
{
    public EntradaProductoRepository(TermalesDbContext context) : base(context) { }

    public async Task<IEnumerable<EntradaProducto>> ObtenerPorProductoAsync(int productoId) =>
        await _dbSet
            .Where(e => e.ProductoId == productoId)
            .OrderByDescending(e => e.Fecha)
            .ToListAsync();
}
