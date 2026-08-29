using Microsoft.EntityFrameworkCore;
using Termales.Common.Utils;
using Termales.DAL.Context;
using Termales.DAL.Interfaces;
using Termales.Entities.Models;

namespace Termales.DAL.Repositories;

public class ComprobanteRepository : GenericRepository<Comprobante>, IComprobanteRepository
{
    public ComprobanteRepository(TermalesDbContext context) : base(context) { }

    public async Task<int> ObtenerUltimoNumeroAsync(string serie)
    {
        var ultimo = await _dbSet
            .Where(c => c.Serie == serie)
            .OrderByDescending(c => c.Numero)
            .Select(c => (int?)c.Numero)
            .FirstOrDefaultAsync();
        return ultimo ?? 0;
    }

    public async Task<IEnumerable<Comprobante>> ObtenerPorFechaAsync(DateOnly fecha, string? tipoAmbiente)
    {
        var (inicio, fin) = PeruTime.DayRange(fecha);

        var query = _dbSet.Where(c => c.FechaEmision >= inicio && c.FechaEmision < fin);
        if (!string.IsNullOrWhiteSpace(tipoAmbiente))
            query = query.Where(c => c.TipoAmbiente == tipoAmbiente);

        return await query.OrderByDescending(c => c.FechaEmision).ToListAsync();
    }

    public async Task<IEnumerable<Comprobante>> ObtenerAnulacionesAsync(DateOnly? desde, DateOnly? hasta)
    {
        var hoy = DateOnly.FromDateTime(PeruTime.Today());
        var (inicio, _) = PeruTime.DayRange(desde ?? hoy);
        var (_, fin) = PeruTime.DayRange(hasta ?? hoy);

        return await _dbSet
            .Where(c => c.Estado == "ANULADO" && c.FechaEmision >= inicio && c.FechaEmision < fin)
            .OrderByDescending(c => c.FechaEmision)
            .ToListAsync();
    }

    public async Task<IEnumerable<Comprobante>> ObtenerPendientesDeCobroAsync() =>
        await _dbSet
            .Include(c => c.Cliente)
            .Where(c => !c.Cobrado && c.Estado != "ANULADO")
            .OrderBy(c => c.FechaEmision)
            .ToListAsync();

    public async Task<Comprobante?> ObtenerConDetalleAsync(int comprobanteId) =>
        await _dbSet
            .Include(c => c.Detalles)
            .Include(c => c.Cliente)
            .Include(c => c.ComprobanteOrigen)
            .FirstOrDefaultAsync(c => c.ComprobanteId == comprobanteId);

    public async Task<IEnumerable<Comprobante>> ObtenerFacturasBoletasAsync(DateOnly fecha)
    {
        var (inicio, fin) = PeruTime.DayRange(fecha);

        return await _dbSet
            .Include(c => c.Detalles)
            .Where(c => (c.TipoComprobante == "FI" || c.TipoComprobante == "BI" || c.TipoComprobante == "NV")
                        && c.FechaEmision >= inicio && c.FechaEmision < fin)
            .OrderByDescending(c => c.FechaEmision)
            .ToListAsync();
    }

    public async Task<IEnumerable<Comprobante>> ObtenerNotasCreditoAsync(DateOnly desde, DateOnly hasta)
    {
        var (inicio, _) = PeruTime.DayRange(desde);
        var (_, fin) = PeruTime.DayRange(hasta);

        return await _dbSet
            .Include(c => c.ComprobanteOrigen)
            .Where(c => c.TipoComprobante == "NC" && c.FechaEmision >= inicio && c.FechaEmision < fin)
            .OrderByDescending(c => c.FechaEmision)
            .ToListAsync();
    }
}
