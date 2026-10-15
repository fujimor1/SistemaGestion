using Termales.DAL.Interfaces;
using Termales.Entities.Models.Inventario;

namespace Termales.DAL.Interfaces.Inventario;

public interface ISalidaProductoRepository : IGenericRepository<SalidaProducto>
{
    Task<IEnumerable<SalidaProducto>> ObtenerPorProductoAsync(int productoId);
    Task<IEnumerable<SalidaProducto>> ObtenerHistorialAsync(DateTime? desde, DateTime? hasta, int? productoId, string? motivo);
}
