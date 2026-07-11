using Termales.Entities.Models.Compras;

namespace Termales.DAL.Interfaces.Compras;

public interface ICompraRepository : IGenericRepository<Compra>
{
    Task<Compra?> ObtenerConDetallesAsync(int compraId);
    Task<(IEnumerable<Compra> Items, int Total)> ObtenerPaginadoAsync(
        int pagina, int tamanoPagina, int? proveedorId, string? estado);
}
