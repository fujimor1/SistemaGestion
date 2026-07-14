using Microsoft.EntityFrameworkCore;
using Termales.DAL.Context;
using Termales.DAL.Interfaces;
using Termales.Entities.Enums;
using Termales.Entities.Models;

namespace Termales.DAL.Repositories;

public class ComprobanteSunatRepository : GenericRepository<ComprobanteSunat>, IComprobanteSunatRepository
{
    public ComprobanteSunatRepository(TermalesDbContext context) : base(context) { }

    public async Task<ComprobanteSunat?> ObtenerPorComprobanteIdAsync(int comprobanteId) =>
        await _dbSet.FindAsync(comprobanteId);

    public async Task<IEnumerable<ComprobanteSunat>> ObtenerPendientesAsync() =>
        await _dbSet
            .Include(c => c.Comprobante)
            .Where(c => c.Estado == EstadoEnvioSunat.Pendiente || c.Estado == EstadoEnvioSunat.ErrorEnvio)
            .OrderBy(c => c.FechaLimiteEnvio)
            .ToListAsync();
}
