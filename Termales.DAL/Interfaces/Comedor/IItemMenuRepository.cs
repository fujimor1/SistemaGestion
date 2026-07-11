using Termales.Entities.Models.Comedor;

namespace Termales.DAL.Interfaces.Comedor;

public interface IItemMenuRepository : IGenericRepository<ItemMenu>
{
    Task<IEnumerable<ItemMenu>> ObtenerActivosAsync();
    Task<IEnumerable<ItemMenu>> ObtenerPorCategoriaAsync(int categoriaMenuId);
    Task<ItemMenu?> ObtenerConRecetaAsync(int id);
}
