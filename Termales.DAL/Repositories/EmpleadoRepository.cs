using Microsoft.EntityFrameworkCore;
using Termales.DAL.Context;
using Termales.DAL.Interfaces;
using Termales.Entities.Models;

namespace Termales.DAL.Repositories;

public class EmpleadoRepository : GenericRepository<Empleado>, IEmpleadoRepository
{
    public EmpleadoRepository(TermalesDbContext context) : base(context) { }

    public async Task<Empleado?> ObtenerPorDniAsync(string dni) =>
        await _dbSet.FirstOrDefaultAsync(e => e.Dni == dni);

    public async Task<IEnumerable<Empleado>> ObtenerActivosAsync() =>
        await _dbSet.Where(e => e.Activo).ToListAsync();

    public async Task<(IEnumerable<Empleado> Items, int Total)> ObtenerPaginadoAsync(
        int pagina, int tamanoPagina, string? busqueda)
    {
        var query = _dbSet.Where(e => e.Activo);

        if (!string.IsNullOrWhiteSpace(busqueda))
            query = query.Where(e =>
                e.Nombres.Contains(busqueda) ||
                e.Apellidos.Contains(busqueda) ||
                e.Dni.Contains(busqueda));

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(e => e.Apellidos)
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .ToListAsync();

        return (items, total);
    }
}
