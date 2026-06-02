using System.Linq.Expressions;

namespace Termales.DAL.Interfaces;

public interface IGenericRepository<T> where T : class
{
    Task<T?> ObtenerPorIdAsync(int id);
    Task<IEnumerable<T>> ObtenerTodosAsync();
    Task<IEnumerable<T>> BuscarAsync(Expression<Func<T, bool>> predicate);
    Task<T> AgregarAsync(T entidad);
    Task ActualizarAsync(T entidad);
    Task EliminarAsync(int id);
    Task<bool> ExisteAsync(Expression<Func<T, bool>> predicate);
    Task<int> ContarAsync(Expression<Func<T, bool>>? predicate = null);
}
