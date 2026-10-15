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
            .Include(s => s.Producto)
            .Where(s => s.ProductoId == productoId)
            .OrderByDescending(s => s.Fecha)
            .ToListAsync();

    public async Task<IEnumerable<SalidaProducto>> ObtenerHistorialAsync(
        DateTime? desde, DateTime? hasta, int? productoId, string? motivo) =>
        await _dbSet
            .Include(s => s.Producto)
            .Where(s =>
                (desde == null || s.Fecha >= desde) &&
                (hasta == null || s.Fecha <= hasta) &&
                (productoId == null || s.ProductoId == productoId) &&
                (motivo == null || s.Motivo == motivo))
            .OrderByDescending(s => s.Fecha)
            .ToListAsync();
}
