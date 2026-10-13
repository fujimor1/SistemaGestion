using Microsoft.EntityFrameworkCore;
using Termales.Common.Utils;
using Termales.DAL.Context;
using Termales.DAL.Interfaces.Inventario;
using Termales.Entities.Models.Inventario;

namespace Termales.DAL.Repositories.Inventario;

public class SalidaInsumoRepository : GenericRepository<SalidaInsumo>, ISalidaInsumoRepository
{
    public SalidaInsumoRepository(TermalesDbContext context) : base(context) { }

    public async Task<IEnumerable<SalidaInsumo>> ObtenerPorInsumoAsync(int insumoId) =>
        await _dbSet
            .Where(s => s.InsumoId == insumoId)
            .OrderByDescending(s => s.Fecha)
            .ToListAsync();

    public async Task<IEnumerable<SalidaInsumo>> ObtenerPorFechaAsync(DateTime fecha)
    {
        // Fecha es UTC real (DateTime.UtcNow). DayRange mapea el día Lima a su ventana UTC.
        var (inicio, fin) = PeruTime.DayRange(fecha);
        return await _dbSet
            .Include(s => s.Insumo)
            .Where(s => s.Fecha >= inicio && s.Fecha < fin)
            .OrderBy(s => s.Insumo.TipoAmbiente)
            .ThenBy(s => s.Insumo.Nombre)
            .ToListAsync();
    }

    public async Task<IEnumerable<SalidaInsumo>> ObtenerHistorialAsync(
        DateTime desde, DateTime hasta, int? insumoId, string? motivo)
    {
        var q = _dbSet.Include(s => s.Insumo).AsQueryable();
        q = q.Where(s => s.Fecha >= desde && s.Fecha < hasta);
        if (insumoId.HasValue) q = q.Where(s => s.InsumoId == insumoId.Value);
        if (!string.IsNullOrWhiteSpace(motivo)) q = q.Where(s => s.Motivo == motivo);
        return await q.OrderByDescending(s => s.Fecha).ToListAsync();
    }
}
