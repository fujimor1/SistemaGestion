using Termales.DAL.Interfaces;
using Termales.Entities.Models.Inventario;

namespace Termales.DAL.Interfaces.Inventario;

public interface ISalidaInsumoRepository : IGenericRepository<SalidaInsumo>
{
    Task<IEnumerable<SalidaInsumo>> ObtenerPorInsumoAsync(int insumoId);
    Task<IEnumerable<SalidaInsumo>> ObtenerPorFechaAsync(DateTime fecha);
}
