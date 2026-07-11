using Microsoft.EntityFrameworkCore;
using Termales.DAL.Context;
using Termales.DAL.Interfaces.Comedor;
using Termales.Entities.Enums;
using Termales.Entities.Models.Comedor;

namespace Termales.DAL.Repositories.Comedor;

public class OrdenRepository : GenericRepository<Orden>, IOrdenRepository
{
    public OrdenRepository(TermalesDbContext context) : base(context) { }

    public async Task<Orden?> ObtenerConDetallesAsync(int ordenId) =>
        await _dbSet
            .Include(o => o.Mesa)
            .Include(o => o.Usuario).ThenInclude(u => u.Empleado)
            .Include(o => o.Detalles.OrderBy(d => d.OrdenDetalleId)).ThenInclude(d => d.ItemMenu)
            .Include(o => o.Detalles.OrderBy(d => d.OrdenDetalleId)).ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(o => o.OrdenId == ordenId);

    public async Task<Orden?> ObtenerActivaPorMesaAsync(int mesaId) =>
        await _dbSet
            .Include(o => o.Detalles.OrderBy(d => d.OrdenDetalleId)).ThenInclude(d => d.ItemMenu)
            .Include(o => o.Detalles.OrderBy(d => d.OrdenDetalleId)).ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(o => o.MesaId == mesaId &&
                o.Estado != EstadoOrden.Pagada &&
                o.Estado != EstadoOrden.Cancelada);

    public async Task<IEnumerable<Orden>> ObtenerPorEstadoAsync(EstadoOrden estado) =>
        await _dbSet
            .Include(o => o.Mesa)
            .Include(o => o.Detalles.OrderBy(d => d.OrdenDetalleId)).ThenInclude(d => d.ItemMenu)
            .Include(o => o.Detalles.OrderBy(d => d.OrdenDetalleId)).ThenInclude(d => d.Producto)
            .Where(o => o.Estado == estado)
            .OrderBy(o => o.FechaApertura)
            .ToListAsync();

    public async Task<IEnumerable<Orden>> ObtenerPorFechaAsync(DateTime fecha) =>
        await _dbSet
            .Include(o => o.Mesa)
            .Include(o => o.Detalles.OrderBy(d => d.OrdenDetalleId)).ThenInclude(d => d.ItemMenu)
            .Include(o => o.Detalles.OrderBy(d => d.OrdenDetalleId)).ThenInclude(d => d.Producto)
            .Where(o => o.FechaApertura.Date == fecha.Date)
            .OrderByDescending(o => o.FechaApertura)
            .ToListAsync();
}
