using Microsoft.EntityFrameworkCore;
using Termales.DAL.Context;
using Termales.DAL.Interfaces.Tienda;
using Termales.Entities.Models.Tienda;

namespace Termales.DAL.Repositories.Tienda;

public class ProductoRepository : GenericRepository<Producto>, IProductoRepository
{
    public ProductoRepository(TermalesDbContext context) : base(context) { }

    public async Task<Producto?> ObtenerPorCodigoBarrasAsync(string codigoBarras) =>
        await _dbSet.FirstOrDefaultAsync(p => p.CodigoBarras == codigoBarras && p.Activo);

    public async Task<IEnumerable<Producto>> ObtenerActivosAsync() =>
        await _dbSet.Where(p => p.Activo).OrderBy(p => p.Nombre).ToListAsync();

    public async Task<(IEnumerable<Producto> Items, int Total)> ObtenerPaginadoAsync(
        int pagina, int tamanoPagina, string? busqueda)
    {
        var query = _dbSet.Where(p => p.Activo);

        if (!string.IsNullOrWhiteSpace(busqueda))
            query = query.Where(p =>
                p.Nombre.Contains(busqueda) ||
                (p.CodigoBarras != null && p.CodigoBarras.Contains(busqueda)));

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(p => p.Nombre)
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .ToListAsync();

        return (items, total);
    }
}
