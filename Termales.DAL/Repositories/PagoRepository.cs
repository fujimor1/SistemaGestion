using Microsoft.EntityFrameworkCore;
using Termales.DAL.Context;
using Termales.DAL.Interfaces;
using Termales.Entities.Models;

namespace Termales.DAL.Repositories;

public class PagoRepository : GenericRepository<Pago>, IPagoRepository
{
    public PagoRepository(TermalesDbContext context) : base(context) { }

    public async Task<Pago?> ObtenerPorReservaAsync(int reservaId) =>
        await _dbSet.FirstOrDefaultAsync(p => p.ReservaId == reservaId);

    public async Task<decimal> ObtenerTotalRecaudadoAsync(DateTime desde, DateTime hasta) =>
        await _dbSet
            .Where(p => p.FechaPago >= desde && p.FechaPago <= hasta)
            .SumAsync(p => p.Monto);
}
