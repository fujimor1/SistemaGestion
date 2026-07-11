using Termales.Entities.Models.Tienda;

namespace Termales.DAL.Interfaces.Tienda;

public interface IProductoRepository : IGenericRepository<Producto>
{
    Task<Producto?> ObtenerPorCodigoBarrasAsync(string codigoBarras);
    Task<IEnumerable<Producto>> ObtenerActivosAsync();
    Task<(IEnumerable<Producto> Items, int Total)> ObtenerPaginadoAsync(int pagina, int tamanoPagina, string? busqueda);
}
