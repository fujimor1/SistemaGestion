using Termales.Entities.Models.Tienda;

namespace Termales.DAL.Interfaces.Tienda;

public interface IProductoRepository : IGenericRepository<Producto>
{
    Task<Producto?> ObtenerPorCodigoBarrasAsync(string codigoBarras);
    Task<IEnumerable<Producto>> ObtenerActivosAsync();
    /// <summary>Todos los productos, activos o no — para pantallas de gestión (Inventario,
    /// selector de Compras), a diferencia de ObtenerActivosAsync que es lo que se puede vender.</summary>
    Task<IEnumerable<Producto>> ObtenerTodosParaGestionAsync();
    Task<(IEnumerable<Producto> Items, int Total)> ObtenerPaginadoAsync(int pagina, int tamanoPagina, string? busqueda);
}
