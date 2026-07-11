using Microsoft.EntityFrameworkCore;
using Termales.DAL.Context;
using Termales.DAL.Interfaces.Compras;
using Termales.Entities.Models.Compras;

namespace Termales.DAL.Repositories.Compras;

public class ProveedorRepository : GenericRepository<Proveedor>, IProveedorRepository
{
    public ProveedorRepository(TermalesDbContext context) : base(context) { }

    public async Task<Proveedor?> ObtenerPorRucAsync(string ruc) =>
        await _dbSet.FirstOrDefaultAsync(p => p.Ruc == ruc);

    public async Task<(IEnumerable<Proveedor> Items, int Total)> ObtenerPaginadoAsync(
        int pagina, int tamanoPagina, string? busqueda)
    {
        var query = _dbSet.Where(p => p.Activo);

        if (!string.IsNullOrWhiteSpace(busqueda))
            query = query.Where(p =>
                p.RazonSocial.Contains(busqueda) ||
                (p.NombreComercial != null && p.NombreComercial.Contains(busqueda)) ||
                p.Ruc.Contains(busqueda));

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(p => p.RazonSocial)
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .ToListAsync();

        return (items, total);
    }
}
