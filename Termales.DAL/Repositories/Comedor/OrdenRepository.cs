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
            .Include(o => o.Mesa).ThenInclude(m => m!.MesasSecundarias)
            .Include(o => o.Usuario).ThenInclude(u => u.Empleado)
            .Include(o => o.Detalles.OrderBy(d => d.OrdenDetalleId)).ThenInclude(d => d.ItemMenu)
            .Include(o => o.Detalles.OrderBy(d => d.OrdenDetalleId)).ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(o => o.OrdenId == ordenId);

    public async Task<Orden?> ObtenerActivaPorMesaAsync(int mesaId) =>
        await _dbSet
            .Include(o => o.Mesa).ThenInclude(m => m!.MesasSecundarias)
            .Include(o => o.Detalles.OrderBy(d => d.OrdenDetalleId)).ThenInclude(d => d.ItemMenu)
            .Include(o => o.Detalles.OrderBy(d => d.OrdenDetalleId)).ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(o => o.MesaId == mesaId &&
                o.Estado != EstadoOrden.Pagada &&
                o.Estado != EstadoOrden.Cancelada);

    public async Task<IEnumerable<Orden>> ObtenerPorEstadoAsync(EstadoOrden estado) =>
        await _dbSet
            .Include(o => o.Mesa).ThenInclude(m => m!.MesasSecundarias)
            .Include(o => o.Detalles.OrderBy(d => d.OrdenDetalleId)).ThenInclude(d => d.ItemMenu)
            .Include(o => o.Detalles.OrderBy(d => d.OrdenDetalleId)).ThenInclude(d => d.Producto)
            .Where(o => o.Estado == estado)
            .OrderBy(o => o.FechaApertura)
            .ToListAsync();

    // soloCreadasPorMozo: la app móvil del mesero consume este mismo endpoint solo para
    // mostrar "sus" pedidos para llevar en curso — un pedido para llevar que un cajero
    // registra desde la web no le sirve de nada ahí y solo genera ruido/confusión, así
    // que para ese caller se filtra por el rol de quien creó el pedido. La web de caja
    // (Administrador/Supervisor) sigue viendo todos, sin filtrar, para poder cobrarlos.
    public async Task<IEnumerable<Orden>> ObtenerLlevarActivasAsync(bool soloCreadasPorMozo) =>
        await _dbSet
            .Include(o => o.Usuario).ThenInclude(u => u.Rol)
            .Include(o => o.Detalles.OrderBy(d => d.OrdenDetalleId)).ThenInclude(d => d.ItemMenu)
            .Include(o => o.Detalles.OrderBy(d => d.OrdenDetalleId)).ThenInclude(d => d.Producto)
            .Where(o => o.TipoEntrega == "llevar" &&
                o.Estado != EstadoOrden.Pagada &&
                o.Estado != EstadoOrden.Cancelada &&
                (!soloCreadasPorMozo || o.Usuario.Rol.Nombre == "Mozo"))
            .OrderBy(o => o.FechaApertura)
            .ToListAsync();

    public async Task<IEnumerable<Orden>> ObtenerPorFechaAsync(DateTime fecha) =>
        await _dbSet
            .Include(o => o.Mesa).ThenInclude(m => m!.MesasSecundarias)
            .Include(o => o.Detalles.OrderBy(d => d.OrdenDetalleId)).ThenInclude(d => d.ItemMenu)
            .Include(o => o.Detalles.OrderBy(d => d.OrdenDetalleId)).ThenInclude(d => d.Producto)
            .Where(o => o.FechaApertura.Date == fecha.Date)
            .OrderByDescending(o => o.FechaApertura)
            .ToListAsync();
}
