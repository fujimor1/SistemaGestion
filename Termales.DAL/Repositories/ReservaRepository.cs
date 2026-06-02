using Microsoft.EntityFrameworkCore;
using Termales.Common.Helpers;
using Termales.DAL.Context;
using Termales.DAL.Interfaces;
using Termales.Entities.Enums;
using Termales.Entities.Models;

namespace Termales.DAL.Repositories;

public class ReservaRepository : GenericRepository<Reserva>, IReservaRepository
{
    public ReservaRepository(TermalesDbContext context) : base(context) { }

    public async Task<Reserva?> ObtenerConDetallesAsync(int reservaId) =>
        await _dbSet
            .Include(r => r.Cliente)
            .Include(r => r.Piscina)
            .Include(r => r.Pago)
            .Include(r => r.ReservaServicios).ThenInclude(rs => rs.Servicio)
            .FirstOrDefaultAsync(r => r.ReservaId == reservaId);

    public async Task<(IEnumerable<Reserva> Items, int Total)> ObtenerPaginadoAsync(FiltroReserva filtro)
    {
        var query = _dbSet
            .Include(r => r.Cliente)
            .Include(r => r.Piscina)
            .AsQueryable();

        if (filtro.ClienteId.HasValue)
            query = query.Where(r => r.ClienteId == filtro.ClienteId.Value);
        if (filtro.PiscinaId.HasValue)
            query = query.Where(r => r.PiscinaId == filtro.PiscinaId.Value);
        if (filtro.FechaDesde.HasValue)
            query = query.Where(r => r.FechaIngreso >= filtro.FechaDesde.Value);
        if (filtro.FechaHasta.HasValue)
            query = query.Where(r => r.FechaIngreso <= filtro.FechaHasta.Value);
        if (!string.IsNullOrWhiteSpace(filtro.Estado) && Enum.TryParse<EstadoReserva>(filtro.Estado, out var estado))
            query = query.Where(r => r.Estado == estado);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(r => r.FechaCreacion)
            .Skip((filtro.Pagina - 1) * filtro.TamanoPagina)
            .Take(filtro.TamanoPagina)
            .ToListAsync();

        return (items, total);
    }

    public async Task<IEnumerable<Reserva>> ObtenerPorClienteAsync(int clienteId) =>
        await _dbSet
            .Include(r => r.Piscina)
            .Where(r => r.ClienteId == clienteId)
            .OrderByDescending(r => r.FechaIngreso)
            .ToListAsync();

    public async Task<IEnumerable<Reserva>> ObtenerPorPiscinaYFechaAsync(int piscinaId, DateTime fecha) =>
        await _dbSet
            .Where(r => r.PiscinaId == piscinaId &&
                        r.FechaIngreso.Date == fecha.Date &&
                        r.Estado != EstadoReserva.Cancelada)
            .ToListAsync();

    public async Task<bool> ExisteConflictoHorarioAsync(
        int piscinaId, DateTime ingreso, DateTime salida, int? reservaIdExcluir = null)
    {
        var query = _dbSet.Where(r =>
            r.PiscinaId == piscinaId &&
            r.Estado != EstadoReserva.Cancelada &&
            r.FechaIngreso < salida &&
            r.FechaSalida > ingreso);

        if (reservaIdExcluir.HasValue)
            query = query.Where(r => r.ReservaId != reservaIdExcluir.Value);

        return await query.AnyAsync();
    }
}
