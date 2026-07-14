using Microsoft.EntityFrameworkCore;
using Termales.DAL.Context;
using Termales.DAL.Interfaces;

namespace Termales.DAL.Repositories;

public class ComprobanteSerieRepository : IComprobanteSerieRepository
{
    private readonly TermalesDbContext _context;

    public ComprobanteSerieRepository(TermalesDbContext context) => _context = context;

    public async Task<int> SiguienteNumeroAsync(string serie, string tipoComprobante)
    {
        // Una sola sentencia atómica: Postgres bloquea la fila durante el UPSERT, así que dos
        // llamadas concurrentes para la misma serie nunca pueden obtener el mismo número.
        var resultado = await _context.Database.SqlQuery<int>(
            $@"INSERT INTO comprobante_series (serie, tipo_comprobante, ultimo_numero)
               VALUES ({serie}, {tipoComprobante}, 1)
               ON CONFLICT (serie) DO UPDATE
                 SET ultimo_numero = comprobante_series.ultimo_numero + 1
               RETURNING ultimo_numero AS ""Value""")
            .ToListAsync();

        return resultado[0];
    }
}
