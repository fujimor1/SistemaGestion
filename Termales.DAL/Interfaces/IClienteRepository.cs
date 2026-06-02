using Termales.Entities.Models;

namespace Termales.DAL.Interfaces;

public interface IClienteRepository : IGenericRepository<Cliente>
{
    Task<Cliente?> ObtenerPorDniAsync(string dni);
    Task<IEnumerable<Cliente>> BuscarPorNombreAsync(string nombre);
    Task<(IEnumerable<Cliente> Items, int Total)> ObtenerPaginadoAsync(int pagina, int tamanoPagina, string? busqueda);
}
