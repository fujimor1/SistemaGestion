using Termales.Entities.Models.Inventario;

namespace Termales.DAL.Interfaces.Inventario;

public interface IEntradaInsumoRepository : IGenericRepository<EntradaInsumo>
{
    Task<IEnumerable<EntradaInsumo>> ObtenerPorInsumoAsync(int insumoId);
}
