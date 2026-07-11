using Termales.Entities.Models.Inventario;

namespace Termales.DAL.Interfaces.Inventario;

public interface IEntradaProductoRepository : IGenericRepository<EntradaProducto>
{
    Task<IEnumerable<EntradaProducto>> ObtenerPorProductoAsync(int productoId);
}
