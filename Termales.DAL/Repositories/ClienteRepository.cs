using Microsoft.EntityFrameworkCore;
using Termales.DAL.Context;
using Termales.DAL.Interfaces;
using Termales.Entities.Models;

namespace Termales.DAL.Repositories;

public class ClienteRepository : GenericRepository<Cliente>, IClienteRepository
{
    public ClienteRepository(TermalesDbContext context) : base(context) { }

    public async Task<Cliente?> ObtenerPorDniAsync(string dni) =>
        await _dbSet.FirstOrDefaultAsync(c => c.Dni == dni);

    public async Task<IEnumerable<Cliente>> BuscarPorNombreAsync(string nombre) =>
        await _dbSet
            .Where(c => c.Activo && (c.Nombres.Contains(nombre) || c.Apellidos.Contains(nombre)))
            .ToListAsync();

    public async Task<(IEnumerable<Cliente> Items, int Total)> ObtenerPaginadoAsync(
        int pagina, int tamanoPagina, string? busqueda)
    {
        var query = _dbSet.Where(c => c.Activo);

        if (!string.IsNullOrWhiteSpace(busqueda))
            query = query.Where(c =>
                c.Nombres.Contains(busqueda) ||
                c.Apellidos.Contains(busqueda) ||
                c.Dni.Contains(busqueda));

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(c => c.Apellidos)
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .ToListAsync();

        return (items, total);
    }
}
