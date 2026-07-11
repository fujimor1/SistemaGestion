using Microsoft.EntityFrameworkCore;
using Termales.BLL.Interfaces;
using Termales.Common.DTOs;
using Termales.DAL.Context;

namespace Termales.BLL.Services;

public class DashboardService : IDashboardService
{
    private readonly TermalesDbContext _db;

    public DashboardService(TermalesDbContext db) => _db = db;

    // ── helpers locales ───────────────────────────────────────────────────────
    private static List<PuntoGraficoDto> PorDia(IEnumerable<(DateTime Fecha, decimal Valor)> raw) =>
        raw.OrderBy(x => x.Fecha)
           .Select(x => new PuntoGraficoDto { Label = x.Fecha.ToString("dd/MM"), Valor = x.Valor })
           .ToList();

    private static List<PuntoGraficoDto> PorHora(IEnumerable<(int Hora, decimal Valor)> raw) =>
        raw.OrderBy(x => x.Hora)
           .Select(x => new PuntoGraficoDto { Label = $"{x.Hora:D2}:00", Valor = x.Valor })
           .ToList();

    // ── Comedor ───────────────────────────────────────────────────────────────

    public async Task<DashboardComedorDto> GetComedorAsync()
    {
        var ahora = DateTime.UtcNow;
        var hoy = ahora.Date;
        var semana = hoy.AddDays(-7);
        var mes = new DateTime(ahora.Year, ahora.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var hace30 = hoy.AddDays(-29);

        var ordenes = _db.Ordenes.AsNoTracking();

        var ingresoHoy = await ordenes
            .Where(o => o.FechaApertura.Date == hoy && (int)o.Estado >= 3)
            .SumAsync(o => (decimal?)o.Total) ?? 0;

        var ingresoSemana = await ordenes
            .Where(o => o.FechaApertura >= semana && (int)o.Estado >= 3)
            .SumAsync(o => (decimal?)o.Total) ?? 0;

        var ingresoMes = await ordenes
            .Where(o => o.FechaApertura >= mes && (int)o.Estado >= 3)
            .SumAsync(o => (decimal?)o.Total) ?? 0;

        var ordenesHoy = await ordenes.CountAsync(o => o.FechaApertura.Date == hoy);
        var ordenesAbiertas = await ordenes.CountAsync(o => (int)o.Estado < 4);

        var ingresosDiaRaw = await ordenes
            .Where(o => o.FechaApertura.Date >= hace30 && (int)o.Estado >= 3)
            .GroupBy(o => o.FechaApertura.Date)
            .Select(g => new { Fecha = g.Key, Valor = g.Sum(o => o.Total) })
            .ToListAsync();

        // Solo platos de cocina (los productos de tienda agregados a una
        // orden no tienen ItemMenuId) — "más vendidos" es un ranking de menú.
        var platosRaw = await _db.OrdenDetalles.AsNoTracking()
            .Where(od => od.Orden.FechaApertura.Date >= hace30 && od.ItemMenuId != null)
            .GroupBy(od => od.ItemMenuId!.Value)
            .Select(g => new { Id = g.Key, Total = g.Sum(od => od.Cantidad) })
            .OrderByDescending(x => x.Total)
            .Take(10)
            .ToListAsync();

        var itemIds = platosRaw.Select(x => x.Id).ToList();
        var items = await _db.ItemsMenu.AsNoTracking()
            .Where(i => itemIds.Contains(i.ItemMenuId))
            .ToDictionaryAsync(i => i.ItemMenuId, i => i.Nombre);

        var horasRaw = await ordenes
            .Where(o => o.FechaApertura.Date >= hace30)
            .GroupBy(o => o.FechaApertura.Hour)
            .Select(g => new { Hora = g.Key, Total = g.Count() })
            .ToListAsync();

        return new DashboardComedorDto
        {
            IngresoHoy = ingresoHoy,
            IngresoSemana = ingresoSemana,
            IngresoMes = ingresoMes,
            OrdenesHoy = ordenesHoy,
            OrdenesAbiertas = ordenesAbiertas,
            IngresosPorDia = PorDia(ingresosDiaRaw.Select(x => (x.Fecha, x.Valor))),
            PlatosMasVendidos = platosRaw.Select(x => new PuntoGraficoDto
            {
                Label = items.GetValueOrDefault(x.Id, $"Item #{x.Id}"),
                Valor = x.Total
            }).ToList(),
            OrdenesParHora = PorHora(horasRaw.Select(x => (x.Hora, (decimal)x.Total))),
        };
    }

    // ── Baños Termales ────────────────────────────────────────────────────────

    public async Task<DashboardBaniosDto> GetBaniosAsync()
    {
        var ahora = DateTime.UtcNow;
        var hoy = ahora.Date;
        var mes = new DateTime(ahora.Year, ahora.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var hace30 = hoy.AddDays(-29);

        var aforos = _db.Aforos.AsNoTracking();
        var comprobantes = _db.Comprobantes.AsNoTracking().Where(c => c.TipoAmbiente == "banio");

        var personasHoy = await aforos
            .Where(a => a.Fecha.Date == hoy)
            .SumAsync(a => (int?)a.OcupacionActual) ?? 0;

        var ingresoHoy = await comprobantes
            .Where(c => c.FechaEmision.Date == hoy)
            .SumAsync(c => (decimal?)c.Total) ?? 0;

        var personasMes = await aforos
            .Where(a => a.Fecha >= mes)
            .SumAsync(a => (int?)a.OcupacionActual) ?? 0;

        var ingresoMes = await comprobantes
            .Where(c => c.FechaEmision >= mes)
            .SumAsync(c => (decimal?)c.Total) ?? 0;

        var personasDiaRaw = await aforos
            .Where(a => a.Fecha.Date >= hace30)
            .GroupBy(a => a.Fecha.Date)
            .Select(g => new { Fecha = g.Key, Valor = g.Max(a => a.OcupacionActual) })
            .ToListAsync();

        var horasRaw = await comprobantes
            .Where(c => c.FechaEmision.Date >= hace30)
            .GroupBy(c => c.FechaEmision.Hour)
            .Select(g => new { Hora = g.Key, Total = g.Count() })
            .ToListAsync();

        var svcRaw = await aforos
            .Where(a => a.Fecha.Date >= hace30)
            .GroupBy(a => a.TipoServicioId)
            .Select(g => new { Id = g.Key, Valor = (decimal)g.Sum(a => a.OcupacionActual) })
            .ToListAsync();

        var svcIds = svcRaw.Select(x => x.Id).ToList();
        var servicios = await _db.TiposServicio.AsNoTracking()
            .Where(s => svcIds.Contains(s.TipoServicioId))
            .ToDictionaryAsync(s => s.TipoServicioId, s => s.Nombre);

        return new DashboardBaniosDto
        {
            PersonasHoy = personasHoy,
            IngresoHoy = ingresoHoy,
            PersonasMes = personasMes,
            IngresoMes = ingresoMes,
            PersonasPorDia = PorDia(personasDiaRaw.Select(x => (x.Fecha, (decimal)x.Valor))),
            PorHora = PorHora(horasRaw.Select(x => (x.Hora, (decimal)x.Total))),
            PorServicio = svcRaw
                .OrderByDescending(x => x.Valor)
                .Select(x => new PuntoGraficoDto
                {
                    Label = servicios.GetValueOrDefault(x.Id, $"Servicio #{x.Id}"),
                    Valor = x.Valor
                }).ToList(),
        };
    }

    // ── Habitaciones ──────────────────────────────────────────────────────────

    public async Task<DashboardHabitacionesDto> GetHabitacionesAsync()
    {
        var ahora = DateTime.UtcNow;
        var hoy = ahora.Date;
        var mes = new DateTime(ahora.Year, ahora.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var hace30 = hoy.AddDays(-29);
        var hace90 = hoy.AddDays(-89);

        // Las habitaciones ya no se reservan con anticipación: se cobran al
        // asignarlas directo desde las cards de Caja (ver
        // ComprobanteService.GenerarComprobanteHabitacion), así que el
        // histórico sale de los comprobantes de tipo_ambiente = "habitacion"
        // y la ocupación actual sale directo de la tabla Habitaciones — no
        // de la tabla Reservas, que en realidad está ligada a Piscina y es
        // un módulo aparte, sin relación con las habitaciones reales.
        var comprobantes = _db.Comprobantes.AsNoTracking()
            .Where(c => c.TipoAmbiente == "habitacion" && c.Estado != "ANULADO" && c.Cobrado);

        var reservasHoy = await comprobantes.CountAsync(c => c.FechaEmision.Date == hoy);
        var reservasMes = await comprobantes.CountAsync(c => c.FechaEmision >= mes);

        var ingresoMes = await comprobantes
            .Where(c => c.FechaEmision >= mes)
            .SumAsync(c => (decimal?)c.Total) ?? 0;

        var totalHabs = await _db.Habitaciones.AsNoTracking().CountAsync(h => h.Activo);
        var ocupadasHoy = await _db.Habitaciones.AsNoTracking().CountAsync(h => h.Activo && h.Ocupado);

        var reservasDiaRaw = await comprobantes
            .Where(c => c.FechaEmision.Date >= hace30)
            .GroupBy(c => c.FechaEmision.Date)
            .Select(g => new { Fecha = g.Key, Valor = (decimal)g.Count() })
            .ToListAsync();

        var ingresosDiaRaw = await comprobantes
            .Where(c => c.FechaEmision.Date >= hace30)
            .GroupBy(c => c.FechaEmision.Date)
            .Select(g => new { Fecha = g.Key, Valor = g.Sum(c => c.Total) })
            .ToListAsync();

        var semanaRaw = await comprobantes
            .Where(c => c.FechaEmision.Date >= hace90)
            .GroupBy(c => c.FechaEmision.DayOfWeek)
            .Select(g => new { Dia = (int)g.Key, Total = (decimal)g.Count() })
            .ToListAsync();

        var diasSemana = new[] { "Dom", "Lun", "Mar", "Mié", "Jue", "Vie", "Sáb" };
        var porDiaSemana = Enumerable.Range(0, 7)
            .Select(i => new PuntoGraficoDto
            {
                Label = diasSemana[i],
                Valor = semanaRaw.FirstOrDefault(x => x.Dia == i)?.Total ?? 0
            }).ToList();

        return new DashboardHabitacionesDto
        {
            ReservasHoy = reservasHoy,
            ReservasMes = reservasMes,
            IngresoMes = ingresoMes,
            HabitacionesDisponibles = totalHabs - ocupadasHoy,
            HabitacionesTotal = totalHabs,
            ReservasPorDia = PorDia(reservasDiaRaw.Select(x => (x.Fecha, x.Valor))),
            PorDiaSemana = porDiaSemana,
            IngresosPorDia = PorDia(ingresosDiaRaw.Select(x => (x.Fecha, x.Valor))),
        };
    }

    // ── Tienda ────────────────────────────────────────────────────────────────

    public async Task<DashboardTiendaDto> GetTiendaAsync()
    {
        var ahora = DateTime.UtcNow;
        var hoy = ahora.Date;
        var semana = hoy.AddDays(-7);
        var mes = new DateTime(ahora.Year, ahora.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var hace30 = hoy.AddDays(-29);

        var comprobantes = _db.Comprobantes.AsNoTracking().Where(c => c.TipoAmbiente == "tienda");

        var ingresoHoy = await comprobantes
            .Where(c => c.FechaEmision.Date == hoy)
            .SumAsync(c => (decimal?)c.Total) ?? 0;

        var ingresoSemana = await comprobantes
            .Where(c => c.FechaEmision >= semana)
            .SumAsync(c => (decimal?)c.Total) ?? 0;

        var ingresoMes = await comprobantes
            .Where(c => c.FechaEmision >= mes)
            .SumAsync(c => (decimal?)c.Total) ?? 0;

        var ventasHoy = await comprobantes.CountAsync(c => c.FechaEmision.Date == hoy);
        var productosTotales = await _db.Productos.AsNoTracking().CountAsync(p => p.Activo);

        var ingresosDiaRaw = await comprobantes
            .Where(c => c.FechaEmision.Date >= hace30)
            .GroupBy(c => c.FechaEmision.Date)
            .Select(g => new { Fecha = g.Key, Valor = g.Sum(c => c.Total) })
            .ToListAsync();

        var horasRaw = await comprobantes
            .Where(c => c.FechaEmision.Date >= hace30)
            .GroupBy(c => c.FechaEmision.Hour)
            .Select(g => new { Hora = g.Key, Total = g.Count() })
            .ToListAsync();

        var stockBajo = await _db.Productos.AsNoTracking()
            .Where(p => p.Activo && p.Stock <= 5)
            .OrderBy(p => p.Stock)
            .Select(p => new StockBajoDto { Nombre = p.Nombre, Stock = p.Stock, Precio = p.Precio })
            .Take(10)
            .ToListAsync();

        return new DashboardTiendaDto
        {
            IngresoHoy = ingresoHoy,
            IngresoSemana = ingresoSemana,
            IngresoMes = ingresoMes,
            VentasHoy = ventasHoy,
            ProductosTotales = productosTotales,
            IngresosPorDia = PorDia(ingresosDiaRaw.Select(x => (x.Fecha, x.Valor))),
            VentasPorHora = PorHora(horasRaw.Select(x => (x.Hora, (decimal)x.Total))),
            ProductosStockBajo = stockBajo,
        };
    }
}
