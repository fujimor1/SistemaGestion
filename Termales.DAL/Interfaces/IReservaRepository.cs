using Termales.Common.Helpers;
using Termales.Entities.Enums;
using Termales.Entities.Models;

namespace Termales.DAL.Interfaces;

public interface IReservaRepository : IGenericRepository<Reserva>
{
    Task<Reserva?> ObtenerConDetallesAsync(int reservaId);
    Task<(IEnumerable<Reserva> Items, int Total)> ObtenerPaginadoAsync(FiltroReserva filtro);
    Task<IEnumerable<Reserva>> ObtenerPorClienteAsync(int clienteId);
    Task<IEnumerable<Reserva>> ObtenerPorPiscinaYFechaAsync(int piscinaId, DateTime fecha);
    Task<bool> ExisteConflictoHorarioAsync(int piscinaId, DateTime ingreso, DateTime salida, int? reservaIdExcluir = null);
}
