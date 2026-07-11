using Termales.Entities.Models.Compras;

namespace Termales.DAL.Interfaces.Compras;

public interface IProveedorRepository : IGenericRepository<Proveedor>
{
    Task<Proveedor?> ObtenerPorRucAsync(string ruc);
    Task<(IEnumerable<Proveedor> Items, int Total)> ObtenerPaginadoAsync(int pagina, int tamanoPagina, string? busqueda);
}
