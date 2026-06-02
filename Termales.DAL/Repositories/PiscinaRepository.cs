using Microsoft.EntityFrameworkCore;
using Termales.DAL.Context;
using Termales.DAL.Interfaces;
using Termales.Entities.Enums;
using Termales.Entities.Models;

namespace Termales.DAL.Repositories;

public class PiscinaRepository : GenericRepository<Piscina>, IPiscinaRepository
{
    public PiscinaRepository(TermalesDbContext context) : base(context) { }

    public async Task<IEnumerable<Piscina>> ObtenerDisponiblesAsync() =>
        await _dbSet.Where(p => p.Disponible).ToListAsync();

    public async Task<IEnumerable<Piscina>> ObtenerDisponiblesEnFechaAsync(DateTime ingreso, DateTime salida)
    {
        var piscinasOcupadas = await _context.Reservas
            .Where(r => r.Estado != EstadoReserva.Cancelada &&
                        r.FechaIngreso < salida &&
                        r.FechaSalida > ingreso)
            .Select(r => r.PiscinaId)
            .Distinct()
            .ToListAsync();

        return await _dbSet
            .Where(p => p.Disponible && !piscinasOcupadas.Contains(p.PiscinaId))
            .ToListAsync();
    }
}
