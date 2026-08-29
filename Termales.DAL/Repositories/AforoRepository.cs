using Microsoft.EntityFrameworkCore;
using Termales.DAL.Context;
using Termales.DAL.Interfaces;
using Termales.Entities.Models;

namespace Termales.DAL.Repositories;

public class AforoRepository : GenericRepository<Aforo>, IAforoRepository
{
    public AforoRepository(TermalesDbContext context) : base(context) { }

    // Aforo.Fecha es un day-key (un registro por día/tipo de servicio, ver
    // AforoService.CrearAsync), así que compararlo por `.Date ==` es correcto — pero
    // la columna es `timestamp with time zone`, así que el parámetro debe tener
    // Kind=Utc explícito o Npgsql lo rechaza ("only UTC is supported"). `fecha` puede
    // llegar con Kind=Unspecified si viene del query string (model binding de ASP.NET)
    // en vez de PeruTime.Today().
    private static DateTime NormalizarDia(DateTime fecha) => DateTime.SpecifyKind(fecha.Date, DateTimeKind.Utc);

    public async Task<Aforo?> ObtenerPorTipoYFechaAsync(int tipoServicioId, DateTime fecha)
    {
        var dia = NormalizarDia(fecha);
        return await _dbSet
            .Include(a => a.TipoServicio)
            .FirstOrDefaultAsync(a => a.TipoServicioId == tipoServicioId && a.Fecha.Date == dia);
    }

    public async Task<IEnumerable<Aforo>> ObtenerPorFechaAsync(DateTime fecha)
    {
        var dia = NormalizarDia(fecha);
        return await _dbSet
            .Include(a => a.TipoServicio)
            .Where(a => a.Fecha.Date == dia)
            .ToListAsync();
    }
}
