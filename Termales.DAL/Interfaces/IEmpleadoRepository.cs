using Termales.Entities.Models;

namespace Termales.DAL.Interfaces;

public interface IEmpleadoRepository : IGenericRepository<Empleado>
{
    Task<Empleado?> ObtenerPorDniAsync(string dni);
    Task<IEnumerable<Empleado>> ObtenerActivosAsync();
    Task<(IEnumerable<Empleado> Items, int Total)> ObtenerPaginadoAsync(int pagina, int tamanoPagina, string? busqueda);
}
