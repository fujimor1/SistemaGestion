using Termales.Entities.Enums;
using Termales.Entities.Models.Comedor;

namespace Termales.DAL.Interfaces.Comedor;

public interface IOrdenRepository : IGenericRepository<Orden>
{
    Task<Orden?> ObtenerConDetallesAsync(int ordenId);
    Task<Orden?> ObtenerActivaPorMesaAsync(int mesaId);
    Task<IEnumerable<Orden>> ObtenerPorEstadoAsync(EstadoOrden estado);
    Task<IEnumerable<Orden>> ObtenerPorFechaAsync(DateTime fecha);
}
