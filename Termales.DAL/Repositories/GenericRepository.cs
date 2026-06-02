using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Termales.DAL.Context;
using Termales.DAL.Interfaces;

namespace Termales.DAL.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly TermalesDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(TermalesDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> ObtenerPorIdAsync(int id) =>
        await _dbSet.FindAsync(id);

    public async Task<IEnumerable<T>> ObtenerTodosAsync() =>
        await _dbSet.ToListAsync();

    public async Task<IEnumerable<T>> BuscarAsync(Expression<Func<T, bool>> predicate) =>
        await _dbSet.Where(predicate).ToListAsync();

    public async Task<T> AgregarAsync(T entidad)
    {
        await _dbSet.AddAsync(entidad);
        return entidad;
    }

    public Task ActualizarAsync(T entidad)
    {
        _dbSet.Update(entidad);
        return Task.CompletedTask;
    }

    public async Task EliminarAsync(int id)
    {
        var entidad = await _dbSet.FindAsync(id);
        if (entidad is not null)
            _dbSet.Remove(entidad);
    }

    public async Task<bool> ExisteAsync(Expression<Func<T, bool>> predicate) =>
        await _dbSet.AnyAsync(predicate);

    public async Task<int> ContarAsync(Expression<Func<T, bool>>? predicate = null) =>
        predicate is null
            ? await _dbSet.CountAsync()
            : await _dbSet.CountAsync(predicate);
}
