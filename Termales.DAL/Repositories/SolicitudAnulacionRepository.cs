using Microsoft.EntityFrameworkCore;
using Termales.Common.Utils;
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
        // FechaResolucion es un instante real (DateTime.UtcNow al aprobar/rechazar, ver
        // SolicitudAnulacionService), así que el rango debe ir en hora Perú, no medianoche
        // UTC directa — antes no se aplicaba ningún offset acá.
        var (inicio, _) = PeruTime.DayRange(desde);
        var (_, fin) = PeruTime.DayRange(hasta);
        return await _dbSet
            .Include(s => s.Comprobante)
            .Include(s => s.NotaCreditoComprobante)
            .Where(s => s.Estado != "Pendiente" && s.FechaResolucion >= inicio && s.FechaResolucion < fin)
            .OrderByDescending(s => s.FechaResolucion)
            .ToListAsync();
    }

    public async Task<SolicitudAnulacion?> ObtenerPorComprobanteAsync(int comprobanteId) =>
        await _dbSet.FirstOrDefaultAsync(s => s.ComprobanteId == comprobanteId && s.Estado == "Pendiente");
}
