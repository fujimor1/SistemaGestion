using Microsoft.EntityFrameworkCore;
using Termales.DAL.Context;
using Termales.DAL.Interfaces.Inventario;
using Termales.Entities.Models.Inventario;

namespace Termales.DAL.Repositories.Inventario;

public class SalidaProductoRepository : GenericRepository<SalidaProducto>, ISalidaProductoRepository
{
    public SalidaProductoRepository(TermalesDbContext context) : base(context) { }

    public async Task<IEnumerable<SalidaProducto>> ObtenerPorProductoAsync(int productoId) =>
        await _dbSet
            .Where(s => s.ProductoId == productoId)
            .OrderByDescending(s => s.Fecha)
            .ToListAsync();
}
