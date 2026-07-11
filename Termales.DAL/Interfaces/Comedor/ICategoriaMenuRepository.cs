using Termales.Entities.Models.Comedor;

namespace Termales.DAL.Interfaces.Comedor;

public interface ICategoriaMenuRepository : IGenericRepository<CategoriaMenu>
{
    Task<IEnumerable<CategoriaMenu>> ObtenerActivasAsync();
}
