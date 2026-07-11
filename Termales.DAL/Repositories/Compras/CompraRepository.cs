using Microsoft.EntityFrameworkCore;
using Termales.DAL.Context;
using Termales.DAL.Interfaces.Compras;
using Termales.Entities.Models.Compras;

namespace Termales.DAL.Repositories.Compras;

public class CompraRepository : GenericRepository<Compra>, ICompraRepository
{
    public CompraRepository(TermalesDbContext context) : base(context) { }

    public async Task<Compra?> ObtenerConDetallesAsync(int compraId) =>
        await _dbSet
            .Include(c => c.Proveedor)
            .Include(c => c.Detalles).ThenInclude(d => d.Insumo)
            .Include(c => c.Detalles).ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(c => c.CompraId == compraId);

    public async Task<(IEnumerable<Compra> Items, int Total)> ObtenerPaginadoAsync(
        int pagina, int tamanoPagina, int? proveedorId, string? estado)
    {
        var query = _dbSet.Include(c => c.Proveedor).AsQueryable();

        if (proveedorId is not null)
            query = query.Where(c => c.ProveedorId == proveedorId);

        if (!string.IsNullOrWhiteSpace(estado))
            query = query.Where(c => c.Estado == estado);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(c => c.FechaEmision)
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .ToListAsync();

        return (items, total);
    }
}
