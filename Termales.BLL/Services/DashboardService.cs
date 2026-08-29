using Microsoft.EntityFrameworkCore;
using Termales.BLL.Interfaces;
using Termales.Common.DTOs;
using Termales.Common.Utils;
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
        var hoy = PeruTime.Today();
        var (inicioHoy, finHoy) = PeruTime.DayRange(hoy);
        var (inicioSemana, _) = PeruTime.DayRange(hoy.AddDays(-7));
        var (inicioMes, _) = PeruTime.DayRange(new DateTime(hoy.Year, hoy.Month, 1));
        var (inicioHace30, _) = PeruTime.DayRange(hoy.AddDays(-29));

        var ordenes = _db.Ordenes.AsNoTracking();

        // FechaApertura es un instante real (UtcNow al abrir la mesa): se filtra por
        // rango [inicio, fin) del día de negocio en Perú, y para agrupar "por día" o
        // "por hora" se desplaza -5h antes de truncar, así el corte cae a medianoche
        // Perú (no UTC) y la hora del gráfico es la hora local, no la UTC.
        var ingresoHoy = await ordenes
            .Where(o => o.FechaApertura >= inicioHoy && o.FechaApertura < finHoy && (int)o.Estado >= 3)
            .SumAsync(o => (decimal?)o.Total) ?? 0;

        var ingresoSemana = await ordenes
            .Where(o => o.FechaApertura >= inicioSemana && (int)o.Estado >= 3)
            .SumAsync(o => (decimal?)o.Total) ?? 0;

        var ingresoMes = await ordenes
            .Where(o => o.FechaApertura >= inicioMes && (int)o.Estado >= 3)
            .SumAsync(o => (decimal?)o.Total) ?? 0;

        var ordenesHoy = await ordenes.CountAsync(o => o.FechaApertura >= inicioHoy && o.FechaApertura < finHoy);
        var ordenesAbiertas = await ordenes.CountAsync(o => (int)o.Estado < 4);

        var ingresosDiaRaw = await ordenes
            .Where(o => o.FechaApertura >= inicioHace30 && (int)o.Estado >= 3)
            .GroupBy(o => o.FechaApertura.AddHours(-5).Date)
            .Select(g => new { Fecha = g.Key, Valor = g.Sum(o => o.Total) })
            .ToListAsync();

        // Solo platos de cocina (los productos de tienda agregados a una
        // orden no tienen ItemMenuId) — "más vendidos" es un ranking de menú.
        var platosRaw = await _db.OrdenDetalles.AsNoTracking()
            .Where(od => od.Orden.FechaApertura >= inicioHace30 && od.ItemMenuId != null)
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
            .Where(o => o.FechaApertura >= inicioHace30)
            .GroupBy(o => o.FechaApertura.AddHours(-5).Hour)
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
        var hoy = PeruTime.Today();
        var (inicioHoy, finHoy) = PeruTime.DayRange(hoy);
        var mesInicioDia = DateTime.SpecifyKind(new DateTime(hoy.Year, hoy.Month, 1), DateTimeKind.Utc);
        var (inicioMes, _) = PeruTime.DayRange(mesInicioDia);
        var hace30 = hoy.AddDays(-29);
        var (inicioHace30, _) = PeruTime.DayRange(hace30);

        // Aforo.Fecha es un day-key (se guarda ya truncado a .Date al crearse, ver
        // AforoService.CrearAsync) — se compara directo contra el día de negocio en
        // Perú, sin rango. Comprobante.FechaCobro/FechaVenta sí son instantes reales.
        var aforos = _db.Aforos.AsNoTracking();
        // Sin este filtro se sumaban comprobantes ANULADOS y Notas de Crédito como si
        // fueran ingreso — una NC anula/reduce una venta anterior, no es venta nueva.
        var comprobantes = _db.Comprobantes.AsNoTracking()
            .Where(c => c.TipoAmbiente == "banio" && c.Estado != "ANULADO" && c.Cobrado && c.TipoComprobante != "NC");

        var personasHoy = await aforos
            .Where(a => a.Fecha.Date == hoy)
            .SumAsync(a => (int?)a.OcupacionActual) ?? 0;

        var ingresoHoy = await comprobantes
            .Where(c => (c.FechaCobro ?? c.FechaVenta) >= inicioHoy && (c.FechaCobro ?? c.FechaVenta) < finHoy)
            .SumAsync(c => (decimal?)c.Total) ?? 0;

        var personasMes = await aforos
            .Where(a => a.Fecha >= mesInicioDia)
            .SumAsync(a => (int?)a.OcupacionActual) ?? 0;

        var ingresoMes = await comprobantes
            .Where(c => (c.FechaCobro ?? c.FechaVenta) >= inicioMes)
            .SumAsync(c => (decimal?)c.Total) ?? 0;

        var personasDiaRaw = await aforos
            .Where(a => a.Fecha >= hace30)
            .GroupBy(a => a.Fecha.Date)
            .Select(g => new { Fecha = g.Key, Valor = g.Max(a => a.OcupacionActual) })
            .ToListAsync();

        var horasRaw = await comprobantes
            .Where(c => (c.FechaCobro ?? c.FechaVenta) >= inicioHace30)
            .GroupBy(c => (c.FechaCobro ?? c.FechaVenta).AddHours(-5).Hour)
            .Select(g => new { Hora = g.Key, Total = g.Count() })
            .ToListAsync();

        var svcRaw = await aforos
            .Where(a => a.Fecha >= hace30)
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
        var hoy = PeruTime.Today();
        var (inicioHoy, finHoy) = PeruTime.DayRange(hoy);
        var (inicioMes, _) = PeruTime.DayRange(new DateTime(hoy.Year, hoy.Month, 1));
        var (inicioHace30, _) = PeruTime.DayRange(hoy.AddDays(-29));
        var (inicioHace90, _) = PeruTime.DayRange(hoy.AddDays(-89));

        // Las habitaciones ya no se reservan con anticipación: se cobran al
        // asignarlas directo desde las cards de Caja (ver
        // ComprobanteService.GenerarComprobanteHabitacion), así que el
        // histórico sale de los comprobantes de tipo_ambiente = "habitacion"
        // y la ocupación actual sale directo de la tabla Habitaciones — no
        // de la tabla Reservas, que en realidad está ligada a Piscina y es
        // un módulo aparte, sin relación con las habitaciones reales.
        var comprobantes = _db.Comprobantes.AsNoTracking()
            .Where(c => c.TipoAmbiente == "habitacion" && c.Estado != "ANULADO" && c.Cobrado
                        && c.TipoComprobante != "NC"); // la NC anula una venta anterior, no es ingreso nuevo

        var reservasHoy = await comprobantes.CountAsync(c => (c.FechaCobro ?? c.FechaVenta) >= inicioHoy && (c.FechaCobro ?? c.FechaVenta) < finHoy);
        var reservasMes = await comprobantes.CountAsync(c => (c.FechaCobro ?? c.FechaVenta) >= inicioMes);

        var ingresoMes = await comprobantes
            .Where(c => (c.FechaCobro ?? c.FechaVenta) >= inicioMes)
            .SumAsync(c => (decimal?)c.Total) ?? 0;

        var totalHabs = await _db.Habitaciones.AsNoTracking().CountAsync(h => h.Activo);
        var ocupadasHoy = await _db.Habitaciones.AsNoTracking().CountAsync(h => h.Activo && h.Ocupado);

        var reservasDiaRaw = await comprobantes
            .Where(c => (c.FechaCobro ?? c.FechaVenta) >= inicioHace30)
            .GroupBy(c => (c.FechaCobro ?? c.FechaVenta).AddHours(-5).Date)
            .Select(g => new { Fecha = g.Key, Valor = (decimal)g.Count() })
            .ToListAsync();

        var ingresosDiaRaw = await comprobantes
            .Where(c => (c.FechaCobro ?? c.FechaVenta) >= inicioHace30)
            .GroupBy(c => (c.FechaCobro ?? c.FechaVenta).AddHours(-5).Date)
            .Select(g => new { Fecha = g.Key, Valor = g.Sum(c => c.Total) })
            .ToListAsync();

        var semanaRaw = await comprobantes
            .Where(c => (c.FechaCobro ?? c.FechaVenta) >= inicioHace90)
            .GroupBy(c => (c.FechaCobro ?? c.FechaVenta).AddHours(-5).DayOfWeek)
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
        var hoy = PeruTime.Today();
        var (inicioHoy, finHoy) = PeruTime.DayRange(hoy);
        var (inicioSemana, _) = PeruTime.DayRange(hoy.AddDays(-7));
        var (inicioMes, _) = PeruTime.DayRange(new DateTime(hoy.Year, hoy.Month, 1));
        var (inicioHace30, _) = PeruTime.DayRange(hoy.AddDays(-29));

        // Sin este filtro se sumaban comprobantes ANULADOS y Notas de Crédito como si
        // fueran ingreso — una NC anula/reduce una venta anterior, no es venta nueva.
        var comprobantes = _db.Comprobantes.AsNoTracking()
            .Where(c => c.TipoAmbiente == "tienda" && c.Estado != "ANULADO" && c.Cobrado && c.TipoComprobante != "NC");

        var ingresoHoy = await comprobantes
            .Where(c => (c.FechaCobro ?? c.FechaVenta) >= inicioHoy && (c.FechaCobro ?? c.FechaVenta) < finHoy)
            .SumAsync(c => (decimal?)c.Total) ?? 0;

        var ingresoSemana = await comprobantes
            .Where(c => (c.FechaCobro ?? c.FechaVenta) >= inicioSemana)
            .SumAsync(c => (decimal?)c.Total) ?? 0;

        var ingresoMes = await comprobantes
            .Where(c => (c.FechaCobro ?? c.FechaVenta) >= inicioMes)
            .SumAsync(c => (decimal?)c.Total) ?? 0;

        var ventasHoy = await comprobantes.CountAsync(c => (c.FechaCobro ?? c.FechaVenta) >= inicioHoy && (c.FechaCobro ?? c.FechaVenta) < finHoy);
        var productosTotales = await _db.Productos.AsNoTracking().CountAsync(p => p.Activo);

        var ingresosDiaRaw = await comprobantes
            .Where(c => (c.FechaCobro ?? c.FechaVenta) >= inicioHace30)
            .GroupBy(c => (c.FechaCobro ?? c.FechaVenta).AddHours(-5).Date)
            .Select(g => new { Fecha = g.Key, Valor = g.Sum(c => c.Total) })
            .ToListAsync();

        var horasRaw = await comprobantes
            .Where(c => (c.FechaCobro ?? c.FechaVenta) >= inicioHace30)
            .GroupBy(c => (c.FechaCobro ?? c.FechaVenta).AddHours(-5).Hour)
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
