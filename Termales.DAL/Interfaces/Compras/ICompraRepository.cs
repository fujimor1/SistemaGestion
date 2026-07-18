using Termales.Entities.Models.Compras;

namespace Termales.DAL.Interfaces.Compras;

public interface ICompraRepository : IGenericRepository<Compra>
{
    Task<Compra?> ObtenerConDetallesAsync(int compraId);
    Task<(IEnumerable<Compra> Items, int Total)> ObtenerPaginadoAsync(
        int pagina, int tamanoPagina, int? proveedorId, string? estado);

    /// <summary>Suma de Compra.Total (excluyendo ANULADA) en el rango [desde, hasta),
    /// sin importar si la compra generó o no un egreso de caja chica.</summary>
    Task<(decimal Total, int Cantidad)> ObtenerResumenAsync(DateTime desde, DateTime hasta);
}
