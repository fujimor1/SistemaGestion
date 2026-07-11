using Microsoft.EntityFrameworkCore;
using Termales.DAL.Context;
using Termales.DAL.Interfaces;
using Termales.Entities.Models;

namespace Termales.DAL.Repositories;

public class SolicitudAnulacionRepository : GenericRepository<SolicitudAnulacion>, ISolicitudAnulacionRepository
{
    public SolicitudAnulacionRepository(TermalesDbContext context) : base(context) { }

    public async Task<IEnumerable<SolicitudAnulacion>> ObtenerPendientesAsync() =>
        await _dbSet
            .Include(s => s.Comprobante)
            .Where(s => s.Estado == "Pendiente")
            .OrderBy(s => s.FechaSolicitud)
            .ToListAsync();

    public async Task<IEnumerable<SolicitudAnulacion>> ObtenerHistorialAsync(DateOnly desde, DateOnly hasta)
    {
        var inicio = desde.ToDateTime(TimeOnly.MinValue);
        var fin    = hasta.ToDateTime(TimeOnly.MaxValue);
        return await _dbSet
            .Include(s => s.Comprobante)
            .Where(s => s.Estado != "Pendiente" && s.FechaResolucion >= inicio && s.FechaResolucion <= fin)
            .OrderByDescending(s => s.FechaResolucion)
            .ToListAsync();
    }

    public async Task<SolicitudAnulacion?> ObtenerPorComprobanteAsync(int comprobanteId) =>
        await _dbSet.FirstOrDefaultAsync(s => s.ComprobanteId == comprobanteId && s.Estado == "Pendiente");
}
