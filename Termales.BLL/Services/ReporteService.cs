using Microsoft.EntityFrameworkCore;
using Termales.BLL.Interfaces;
using Termales.Common.DTOs.Reporte;
using Termales.Common.Helpers;
using Termales.DAL.Context;
using Termales.Entities.Enums;

namespace Termales.BLL.Services;

public class ReporteService : IReporteService
{
    private readonly TermalesDbContext _db;

    public ReporteService(TermalesDbContext db) => _db = db;

    private static (DateTime inicio, DateTime fin) ParseMes(string mes)
    {
        var partes = mes.Split('-');
        if (partes.Length == 2
            && int.TryParse(partes[0], out var year)
            && int.TryParse(partes[1], out var month))
        {
            var inicio = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
            return (inicio, inicio.AddMonths(1));
        }
        var hoy = DateTime.UtcNow;
        var fallback = new DateTime(hoy.Year, hoy.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        return (fallback, fallback.AddMonths(1));
    }

    // ── Reporte de Comprobantes ──────────────────────────────────────────────

    public async Task<ReporteComprobantesDto> ReporteComprobantesAsync(string mes)
    {
        var (inicio, fin) = ParseMes(mes);

        var comprobantes = await _db.Comprobantes.AsNoTracking()
            .Where(c => c.FechaEmision >= inicio && c.FechaEmision < fin)
            .OrderBy(c => c.FechaEmision)
            .ToListAsync();

        var emitidos = comprobantes.Where(c => c.Estado != "ANULADO").ToList();
        var anulados = comprobantes.Where(c => c.Estado == "ANULADO").ToList();

        var porDia = comprobantes
            .GroupBy(c => DateOnly.FromDateTime(c.FechaEmision))
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                var em = g.Where(c => c.Estado != "ANULADO").ToList();
                var an = g.Where(c => c.Estado == "ANULADO").ToList();
                return new ResumenDiarioComprobanteDto
                {
                    Fecha            = g.Key,
                    CantidadNV       = em.Count(c => c.TipoComprobante == "NV"),
                    CantidadBI       = em.Count(c => c.TipoComprobante == "BI"),
                    CantidadFI       = em.Count(c => c.TipoComprobante == "FI"),
                    CantidadNC       = em.Count(c => c.TipoComprobante == "NC"),
                    CantidadAnulados = an.Count,
                    MontoEmitido     = em.Sum(c => c.Total),
                    MontoAnulado     = an.Sum(c => c.Total),
                    MontoNeto        = em.Sum(c => c.Total),
                };
            }).ToList();

        var detalle = comprobantes.Select(c => new DetalleComprobanteReporteDto
        {
            NumeroFormateado = $"{c.Serie}-{c.Numero:D5}",
            TipoComprobante  = c.TipoComprobante,
            TipoAmbiente     = c.TipoAmbiente,
            ClienteNombre    = c.ClienteNombre ?? c.ClienteRazonSocial,
            Total            = c.Total,
            Estado           = c.Estado,
            FechaEmision     = c.FechaEmision,
            MotivoAnulacion  = c.MotivoAnulacion,
            AutorizadoPor    = c.AutorizadoPor,
        }).ToList();

        return new ReporteComprobantesDto
        {
            Mes               = mes,
            TotalEmitidos     = emitidos.Count,
            TotalAnulados     = anulados.Count,
            TotalNV           = emitidos.Count(c => c.TipoComprobante == "NV"),
            TotalBI           = emitidos.Count(c => c.TipoComprobante == "BI"),
            TotalFI           = emitidos.Count(c => c.TipoComprobante == "FI"),
            TotalNC           = emitidos.Count(c => c.TipoComprobante == "NC"),
            MontoTotalEmitido = emitidos.Sum(c => c.Total),
            MontoTotalAnulado = anulados.Sum(c => c.Total),
            MontoNeto         = emitidos.Sum(c => c.Total),
            PorDia            = porDia,
            Detalle           = detalle,
        };
    }

    // ── Reporte de Caja ──────────────────────────────────────────────────────

    public async Task<ReporteCajaDto> ReporteCajaAsync(string mes)
    {
        var (inicio, fin) = ParseMes(mes);

        var aperturas = await _db.AperturasCaja.AsNoTracking()
            .Where(a => a.Fecha >= inicio && a.Fecha < fin)
            .ToListAsync();

        var cierres = await _db.CierresCaja.AsNoTracking()
            .Where(c => c.Fecha >= inicio && c.Fecha < fin)
            .ToListAsync();

        var egresos = await _db.EgresosCajaChica.AsNoTracking()
            .Where(e => e.Fecha >= inicio && e.Fecha < fin)
            .ToListAsync();

        var ventasRaw = await _db.Comprobantes.AsNoTracking()
            .Where(c => c.FechaEmision >= inicio && c.FechaEmision < fin && c.Estado != "ANULADO" && c.Cobrado)
            .Select(c => new { c.FechaEmision, c.Total })
            .ToListAsync();

        var diasConDatos = new SortedSet<DateOnly>(
            aperturas.Select(a => DateOnly.FromDateTime(a.Fecha))
            .Concat(cierres.Select(c => DateOnly.FromDateTime(c.Fecha)))
            .Concat(egresos.Select(e => DateOnly.FromDateTime(e.Fecha)))
            .Concat(ventasRaw.Select(c => DateOnly.FromDateTime(c.FechaEmision)))
        );

        var porDia = diasConDatos.Select(fecha =>
        {
            var apertura    = aperturas.FirstOrDefault(a => DateOnly.FromDateTime(a.Fecha) == fecha);
            var cierre      = cierres.FirstOrDefault(c => DateOnly.FromDateTime(c.Fecha) == fecha);
            var egresosDia  = egresos.Where(e => DateOnly.FromDateTime(e.Fecha) == fecha).Sum(e => e.Monto);
            var ventas      = ventasRaw.Where(c => DateOnly.FromDateTime(c.FechaEmision) == fecha).Sum(c => c.Total);

            var efectivo      = cierre?.EfectivoFisico ?? 0;
            var yape          = cierre?.YapeFisico ?? 0;
            var transferencia = cierre?.TransferenciaFisico ?? 0;
            var totalContado  = efectivo + yape + transferencia;
            var diferencia    = cierre is not null ? cierre.Diferencia : 0;

            return new ResumenDiarioCajaDto
            {
                Fecha                = fecha,
                TieneApertura        = apertura is not null,
                MontoApertura        = apertura?.MontoInicial ?? 0,
                VentasSistema        = ventas,
                EgresosCajaChica     = egresosDia,
                TieneCierre          = cierre is not null,
                EfectivoContado      = efectivo,
                YapeContado          = yape,
                TransferenciaContado = transferencia,
                TotalContado         = totalContado,
                Diferencia           = diferencia,
                Estado               = cierre is not null ? "Cerrada"
                                       : apertura is not null ? "Abierta"
                                       : "Sin apertura",
            };
        }).ToList();

        return new ReporteCajaDto
        {
            Mes                       = mes,
            DiasConApertura           = porDia.Count(d => d.TieneApertura),
            DiasConCierre             = porDia.Count(d => d.TieneCierre),
            TotalVentasSistema        = porDia.Sum(d => d.VentasSistema),
            TotalEgresosCajaChica     = porDia.Sum(d => d.EgresosCajaChica),
            TotalEfectivoContado      = porDia.Sum(d => d.EfectivoContado),
            TotalYapeContado          = porDia.Sum(d => d.YapeContado),
            TotalTransferenciaContado = porDia.Sum(d => d.TransferenciaContado),
            TotalContado              = porDia.Sum(d => d.TotalContado),
            DiferenciaTotal           = porDia.Sum(d => d.Diferencia),
            PorDia                    = porDia,
        };
    }

    // ── Registro de Compras (SUNAT) ──────────────────────────────────────────

    public async Task<RegistroComprasDto> ReporteComprasAsync(string mes)
    {
        var (inicio, fin) = ParseMes(mes);

        var compras = await _db.Compras.AsNoTracking()
            .Include(c => c.Proveedor)
            .Where(c => c.FechaEmision >= inicio && c.FechaEmision < fin)
            .OrderBy(c => c.FechaEmision)
            .ToListAsync();

        var vigentes = compras.Where(c => c.Estado != "ANULADA").ToList();

        var detalle = compras.Select(c => new DetalleCompraReporteDto
        {
            Ruc             = c.Proveedor.Ruc,
            RazonSocial     = c.Proveedor.RazonSocial,
            TipoComprobante = c.TipoComprobante,
            Serie           = c.Serie,
            Numero          = c.Numero,
            FechaEmision    = c.FechaEmision,
            TotalGravada    = c.TotalGravada,
            Igv             = c.Igv,
            Total           = c.Total,
            Estado          = c.Estado,
        }).ToList();

        return new RegistroComprasDto
        {
            Mes               = mes,
            TotalRegistros    = vigentes.Count,
            MontoTotalGravada = vigentes.Sum(c => c.TotalGravada),
            MontoTotalIgv     = vigentes.Sum(c => c.Igv),
            MontoTotal        = vigentes.Sum(c => c.Total),
            Detalle           = detalle,
        };
    }

    // ── Inventario (valorización) ────────────────────────────────────────────

    public async Task<ReporteInventarioDto> ReporteInventarioAsync()
    {
        var insumos = await _db.Insumos.AsNoTracking()
            .Where(i => i.Activo)
            .ToListAsync();

        var detalle = insumos.Select(i => new ValorizacionInsumoDto
        {
            Nombre           = i.Nombre,
            TipoAmbiente     = i.TipoAmbiente,
            TipoArticulo     = i.TipoArticulo,
            Unidad           = i.Unidad,
            StockActual      = i.StockActual,
            PrecioReferencia = i.PrecioReferencia,
        }).OrderByDescending(d => d.Valorizacion).ToList();

        return new ReporteInventarioDto
        {
            ValorizacionTotal = detalle.Sum(d => d.Valorizacion),
            Detalle           = detalle,
        };
    }

    // ── Ventas por categoría (Comedor) ────────────────────────────────────────

    public async Task<ReporteVentasCategoriaDto> ReporteVentasCategoriaAsync(string mes)
    {
        var (inicio, fin) = ParseMes(mes);

        var detalles = await _db.OrdenDetalles.AsNoTracking()
            .Include(d => d.ItemMenu).ThenInclude(i => i!.Categoria)
            .Include(d => d.Comprobante)
            .Where(d => d.ComprobanteId != null && d.ItemMenuId != null &&
                        d.Comprobante!.FechaEmision >= inicio && d.Comprobante.FechaEmision < fin &&
                        d.Comprobante.Estado != "ANULADO")
            .ToListAsync();

        var porCategoria = detalles
            .GroupBy(d => d.ItemMenu!.Categoria?.Nombre ?? "Sin categoría")
            .Select(g => new VentaCategoriaDto
            {
                Categoria       = g.Key,
                CantidadVendida = g.Sum(d => d.Cantidad),
                MontoTotal      = g.Sum(d => d.Subtotal),
            })
            .OrderByDescending(v => v.MontoTotal)
            .ToList();

        return new ReporteVentasCategoriaDto
        {
            Mes        = mes,
            MontoTotal = porCategoria.Sum(v => v.MontoTotal),
            Detalle    = porCategoria,
        };
    }

    // ── Productos más vendidos (todas las ambientes) ───────────────────────────

    public async Task<ReporteProductosMasVendidosDto> ReporteProductosMasVendidosAsync(string mes)
    {
        var (inicio, fin) = ParseMes(mes);

        var detalles = await _db.ComprobanteDetalles.AsNoTracking()
            .Include(d => d.Comprobante)
            .Where(d => d.Comprobante!.FechaEmision >= inicio && d.Comprobante.FechaEmision < fin &&
                        d.Comprobante.Estado != "ANULADO")
            .ToListAsync();

        var porProducto = detalles
            .GroupBy(d => d.Descripcion)
            .Select(g => new ProductoMasVendidoDto
            {
                Descripcion     = g.Key,
                CantidadVendida = g.Sum(d => d.Cantidad),
                MontoTotal      = g.Sum(d => d.Subtotal),
            })
            .OrderByDescending(p => p.CantidadVendida)
            .ToList();

        return new ReporteProductosMasVendidosDto { Mes = mes, Detalle = porProducto };
    }

    // ── Utilidad (Comedor + Tienda) ─────────────────────────────────────────────

    public async Task<ReporteUtilidadDto> ReporteUtilidadAsync(string mes)
    {
        var (inicio, fin) = ParseMes(mes);

        var detallesComedor = await _db.OrdenDetalles.AsNoTracking()
            .Include(d => d.ItemMenu).ThenInclude(i => i!.Receta).ThenInclude(r => r.Insumo)
            .Include(d => d.Comprobante)
            .Where(d => d.ComprobanteId != null && d.ItemMenuId != null &&
                        d.Comprobante!.FechaEmision >= inicio && d.Comprobante.FechaEmision < fin &&
                        d.Comprobante.Estado != "ANULADO")
            .ToListAsync();

        var filasComedor = detallesComedor.Select(d =>
        {
            var costoUnitario = d.ItemMenu!.Receta.Sum(r =>
                ConversionUnidades.RecetaAStockInsumo(r.Cantidad, r.Insumo.Unidad) * r.Insumo.PrecioReferencia);
            var costoTotal = Math.Round(costoUnitario * d.Cantidad, 2);
            return new UtilidadDetalleDto
            {
                Nombre          = d.ItemMenu.Nombre,
                Ambiente        = "comedor",
                CantidadVendida = d.Cantidad,
                Ingreso         = d.Subtotal,
                Costo           = costoTotal,
                Utilidad        = Math.Round(d.Subtotal - costoTotal, 2),
            };
        }).ToList();

        // Tienda no tiene FK exacta de línea de venta a Producto (ComprobanteDetalle
        // solo guarda la descripción) — se empareja por nombre exacto, que es como
        // GenerarComprobanteTienda arma la descripción de cada línea.
        var costosPorNombre = await _db.Productos.AsNoTracking()
            .ToDictionaryAsync(p => p.Nombre, p => p.PrecioCompra);

        var detallesTienda = await _db.ComprobanteDetalles.AsNoTracking()
            .Include(d => d.Comprobante)
            .Where(d => d.Comprobante!.TipoAmbiente == "tienda" &&
                        d.Comprobante.FechaEmision >= inicio && d.Comprobante.FechaEmision < fin &&
                        d.Comprobante.Estado != "ANULADO")
            .ToListAsync();

        var filasTienda = detallesTienda.Select(d =>
        {
            var costoUnitario = costosPorNombre.GetValueOrDefault(d.Descripcion, 0);
            var costoTotal    = Math.Round(costoUnitario * d.Cantidad, 2);
            return new UtilidadDetalleDto
            {
                Nombre          = d.Descripcion,
                Ambiente        = "tienda",
                CantidadVendida = d.Cantidad,
                Ingreso         = d.Subtotal,
                Costo           = costoTotal,
                Utilidad        = Math.Round(d.Subtotal - costoTotal, 2),
            };
        }).ToList();

        var todas = filasComedor.Concat(filasTienda).OrderByDescending(f => f.Utilidad).ToList();

        return new ReporteUtilidadDto
        {
            Mes           = mes,
            IngresoTotal  = todas.Sum(f => f.Ingreso),
            CostoTotal    = todas.Sum(f => f.Costo),
            UtilidadTotal = todas.Sum(f => f.Utilidad),
            Detalle       = todas,
        };
    }

    // ── Ventas por cajero/mesero ─────────────────────────────────────────────

    public async Task<ReportePersonalDto> ReportePersonalAsync(string mes)
    {
        var (inicio, fin) = ParseMes(mes);

        var comprobantes = await _db.Comprobantes.AsNoTracking()
            .Where(c => c.FechaEmision >= inicio && c.FechaEmision < fin && c.Estado != "ANULADO")
            .Select(c => new { c.Cajero, c.Total })
            .ToListAsync();

        var porCajero = comprobantes
            .GroupBy(c => c.Cajero ?? "Sin identificar")
            .Select(g => new VentasPorCajeroDto
            {
                Cajero         = g.Key,
                CantidadVentas = g.Count(),
                MontoTotal     = g.Sum(c => c.Total),
            })
            .OrderByDescending(v => v.MontoTotal)
            .ToList();

        return new ReportePersonalDto
        {
            Mes        = mes,
            MontoTotal = porCajero.Sum(v => v.MontoTotal),
            Detalle    = porCajero,
        };
    }

    // ── Catálogo de precios vigentes ─────────────────────────────────────────

    public async Task<CatalogoDto> ObtenerCatalogoAsync()
    {
        var tienda = await _db.Productos.AsNoTracking()
            .Where(p => p.Activo)
            .Select(p => new CatalogoItemDto { Nombre = p.Nombre, Precio = p.Precio })
            .ToListAsync();

        var comedor = await _db.ItemsMenu.AsNoTracking()
            .Include(i => i.Categoria)
            .Where(i => i.Activo)
            .Select(i => new CatalogoItemDto { Nombre = i.Nombre, Categoria = i.Categoria.Nombre, Precio = i.Precio })
            .ToListAsync();

        var baniosTipos = await _db.TiposServicio.AsNoTracking()
            .Where(t => t.Activo)
            .Select(t => new CatalogoItemDto { Nombre = t.Nombre, Categoria = "Individual", Precio = t.PrecioPorPersona })
            .ToListAsync();

        var baniosPaquetes = await _db.PaquetesBanio.AsNoTracking()
            .Where(p => p.Activo)
            .Select(p => new CatalogoItemDto { Nombre = p.Nombre, Categoria = "Paquete", Precio = p.Precio })
            .ToListAsync();

        var habitaciones = await _db.Habitaciones.AsNoTracking()
            .Where(h => h.Activo)
            .Select(h => new CatalogoItemDto { Nombre = h.Nombre, Precio = h.Precio })
            .ToListAsync();

        return new CatalogoDto
        {
            Tienda       = tienda,
            Comedor      = comedor,
            Banios       = baniosTipos.Concat(baniosPaquetes).ToList(),
            Habitaciones = habitaciones,
        };
    }

    // ── Pagos por QR (Yape/Plin) ──────────────────────────────────────────────

    public async Task<ReportePagoQrDto> ReportePagoQrAsync(string mes)
    {
        var (inicio, fin) = ParseMes(mes);

        var comprobantes = await _db.Comprobantes.AsNoTracking()
            .Where(c => c.FechaEmision >= inicio && c.FechaEmision < fin &&
                        c.Estado != "ANULADO" && c.MetodoPago == MetodoPago.YapePlin && c.Cobrado)
            .OrderBy(c => c.FechaEmision)
            .ToListAsync();

        var detalle = comprobantes.Select(c => new DetalleComprobanteReporteDto
        {
            NumeroFormateado = $"{c.Serie}-{c.Numero:D5}",
            TipoComprobante  = c.TipoComprobante,
            TipoAmbiente     = c.TipoAmbiente,
            ClienteNombre    = c.ClienteNombre ?? c.ClienteRazonSocial,
            Total            = c.Total,
            Estado           = c.Estado,
            FechaEmision     = c.FechaEmision,
        }).ToList();

        return new ReportePagoQrDto
        {
            Mes                = mes,
            TotalTransacciones = detalle.Count,
            MontoTotal         = detalle.Sum(d => d.Total),
            Detalle            = detalle,
        };
    }

    // ── Reporte de comandas ───────────────────────────────────────────────────

    public async Task<ReporteComandasDto> ReporteComandasAsync(string mes)
    {
        var (inicio, fin) = ParseMes(mes);

        var ordenes = await _db.Ordenes.AsNoTracking()
            .Include(o => o.Mesa)
            .Include(o => o.Detalles)
            .Where(o => o.FechaApertura >= inicio && o.FechaApertura < fin)
            .OrderByDescending(o => o.FechaApertura)
            .ToListAsync();

        var detalle = ordenes.Select(o => new ComandaDetalleDto
        {
            OrdenId         = o.OrdenId,
            NumeroMesa      = o.Mesa?.Numero ?? 0,
            FechaApertura   = o.FechaApertura,
            FechaCierre     = o.FechaCierre,
            DuracionMinutos = o.FechaCierre.HasValue
                ? Math.Round((decimal)(o.FechaCierre.Value - o.FechaApertura).TotalMinutes, 1)
                : null,
            CantidadItems   = o.Detalles.Sum(d => d.Cantidad),
            Total           = o.Total,
            Estado          = o.Estado.ToString(),
        }).ToList();

        var duraciones = detalle.Where(d => d.DuracionMinutos.HasValue).Select(d => d.DuracionMinutos!.Value).ToList();

        return new ReporteComandasDto
        {
            Mes                   = mes,
            TotalComandas         = detalle.Count,
            TiempoPromedioMinutos = duraciones.Count > 0 ? Math.Round(duraciones.Average(), 1) : 0,
            Detalle               = detalle,
        };
    }
}
