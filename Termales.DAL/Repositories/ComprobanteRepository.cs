using Microsoft.EntityFrameworkCore;
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

    // Perú es UTC-5 fijo (sin horario de verano). FechaEmision se guarda en UTC,
    // pero el "día" de negocio que filtra el cajero es el día calendario en Perú,
    // así que el rango de un día Perú hay que expresarlo en UTC sumando 5 horas
    // (00:00 Perú = 05:00 UTC del mismo día calendario).
    private static readonly TimeSpan OffsetPeru = TimeSpan.FromHours(5);
    private static DateOnly HoyPeru() => DateOnly.FromDateTime(DateTime.UtcNow - OffsetPeru);

    public async Task<IEnumerable<Comprobante>> ObtenerPorFechaAsync(DateOnly fecha, string? tipoAmbiente)
    {
        var inicio = fecha.ToDateTime(TimeOnly.MinValue) + OffsetPeru;
        var fin    = fecha.ToDateTime(TimeOnly.MaxValue) + OffsetPeru;

        var query = _dbSet.Where(c => c.FechaEmision >= inicio && c.FechaEmision <= fin);
        if (!string.IsNullOrWhiteSpace(tipoAmbiente))
            query = query.Where(c => c.TipoAmbiente == tipoAmbiente);

        return await query.OrderByDescending(c => c.FechaEmision).ToListAsync();
    }

    public async Task<IEnumerable<Comprobante>> ObtenerAnulacionesAsync(DateOnly? desde, DateOnly? hasta)
    {
        var inicio = (desde ?? HoyPeru()).ToDateTime(TimeOnly.MinValue) + OffsetPeru;
        var fin    = (hasta ?? HoyPeru()).ToDateTime(TimeOnly.MaxValue) + OffsetPeru;

        return await _dbSet
            .Where(c => c.Estado == "ANULADO" && c.FechaEmision >= inicio && c.FechaEmision <= fin)
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
            .FirstOrDefaultAsync(c => c.ComprobanteId == comprobanteId);
}
