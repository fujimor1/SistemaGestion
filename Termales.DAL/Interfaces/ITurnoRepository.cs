using Termales.Entities.Models;

namespace Termales.DAL.Interfaces;

public interface ITurnoRepository : IGenericRepository<Turno>
{
    Task<Turno?> ObtenerConDetallesAsync(int turnoId);
    Task<IEnumerable<Turno>> ObtenerPorTipoYFechaAsync(int tipoServicioId, DateTime fecha);
}
