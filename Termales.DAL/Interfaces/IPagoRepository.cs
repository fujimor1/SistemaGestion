using Termales.Entities.Models;

namespace Termales.DAL.Interfaces;

public interface IPagoRepository : IGenericRepository<Pago>
{
    Task<Pago?> ObtenerPorReservaAsync(int reservaId);
    Task<decimal> ObtenerTotalRecaudadoAsync(DateTime desde, DateTime hasta);
}
