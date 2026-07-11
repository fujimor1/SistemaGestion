using Termales.Entities.Models.Inventario;

namespace Termales.DAL.Interfaces.Inventario;

public interface IInsumoRepository : IGenericRepository<Insumo>
{
    Task<IEnumerable<Insumo>> ObtenerPorAmbienteAsync(string tipoAmbiente);
    Task<IEnumerable<Insumo>> ObtenerConEntradasAsync(string tipoAmbiente);
}
