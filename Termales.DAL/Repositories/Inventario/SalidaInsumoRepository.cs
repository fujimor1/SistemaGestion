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
        // Fecha es un instante real (default DateTime.UtcNow al registrar la salida),
        // no un day-key: se filtra por el rango [inicio, fin) del día de negocio en
        // Perú en vez de compararlo por `.Date ==` crudo.
        var (inicio, fin) = PeruTime.DayRange(fecha);
        return await _dbSet
            .Include(s => s.Insumo)
            .Where(s => s.Fecha >= inicio && s.Fecha < fin)
            .OrderBy(s => s.Insumo.TipoAmbiente)
            .ThenBy(s => s.Insumo.Nombre)
            .ToListAsync();
    }
}
