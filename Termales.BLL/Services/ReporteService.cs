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

    // Perú es UTC-5 fijo (sin horario de verano): medianoche en Lima = 05:00 UTC del mismo día.
    private static readonly TimeSpan OffsetPeru = TimeSpan.FromHours(5);

    private static (DateTime inicio, DateTime fin) ParseDia(string fecha)
    {
        if (DateOnly.TryParse(fecha, out var dia))
        {
            var inicio = dia.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc) + OffsetPeru;
            return (inicio, inicio.AddDays(1));
        }
        var hoyLima = DateOnly.FromDateTime(DateTime.UtcNow - OffsetPeru);
        var fallbackInicio = hoyLima.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc) + OffsetPeru;
        return (fallbackInicio, fallbackInicio.AddDays(1));
    }

    // ── Reporte de Comprobantes ──────────────────────────────────────────────

    private static List<ResumenDiarioComprobanteDto> AgruparPorDia(List<Termales.Entities.Models.Comprobante> comprobantes) =>
        comprobantes
            .GroupBy(c => DateOnly.FromDateTime(c.FechaEmision))
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                var vigentes = g.Where(c => c.Estado != "ANULADO").ToList();
                // La Nota de Crédito anula/reduce una venta anterior — no es un ingreso nuevo,
                // así que se cuenta (CantidadNC) pero no se suma al monto emitido/neto.
                var ingresos = vigentes.Where(c => c.TipoComprobante != "NC").ToList();
                var an = g.Where(c => c.Estado == "ANULADO").ToList();
                return new ResumenDiarioComprobanteDto
                {
                    Fecha            = g.Key,
                    CantidadNV       = ingresos.Count(c => c.TipoComprobante == "NV"),
                    CantidadBI       = ingresos.Count(c => c.TipoComprobante == "BI"),
                    CantidadFI       = ingresos.Count(c => c.TipoComprobante == "FI"),
                    CantidadNC       = vigentes.Count(c => c.TipoComprobante == "NC"),
                    CantidadAnulados = an.Count,
                    MontoEmitido     = ingresos.Sum(c => c.Total),
                    MontoAnulado     = an.Sum(c => c.Total),
                    MontoNeto        = ingresos.Sum(c => c.Total),
                };
            }).ToList();

    public async Task<ReporteComprobantesDto> ReporteComprobantesAsync(string desde, string hasta)
    {
        var inicio = ParseDia(desde).inicio;
        var fin    = ParseDia(hasta).fin;

        var comprobantes = await _db.Comprobantes.AsNoTracking()
            .Where(c => c.FechaEmision >= inicio && c.FechaEmision < fin)
            .OrderBy(c => c.FechaEmision)
            .ToListAsync();

        var vigentes = comprobantes.Where(c => c.Estado != "ANULADO").ToList();
        // La Nota de Crédito anula/reduce una venta anterior, no es ingreso nuevo — se
        // cuenta aparte (TotalNC) pero no entra en los montos de "emitidos"/"neto".
        var emitidos = vigentes.Where(c => c.TipoComprobante != "NC").ToList();
        var anulados = comprobantes.Where(c => c.Estado == "ANULADO").ToList();

        var porDia = AgruparPorDia(comprobantes);

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
            Desde             = desde,
            Hasta             = hasta,
            TotalEmitidos     = emitidos.Count,
            TotalAnulados     = anulados.Count,
            TotalNV           = emitidos.Count(c => c.TipoComprobante == "NV"),
            TotalBI           = emitidos.Count(c => c.TipoComprobante == "BI"),
            TotalFI           = emitidos.Count(c => c.TipoComprobante == "FI"),
            TotalNC           = vigentes.Count(c => c.TipoComprobante == "NC"),
            MontoTotalEmitido = emitidos.Sum(c => c.Total),
            MontoTotalAnulado = anulados.Sum(c => c.Total),
            MontoNeto         = emitidos.Sum(c => c.Total),
            PorDia            = porDia,
            Detalle           = detalle,
        };
    }

    /// <summary>Ventas netas por día para un rango de fechas arbitrario (no necesariamente un mes
    /// calendario) — usado por el gráfico de ventas en su vista "Día".</summary>
    public async Task<List<ResumenDiarioComprobanteDto>> ReporteVentasPorRangoAsync(string desde, string hasta)
    {
        var inicio = ParseDia(desde).inicio;
        var fin    = ParseDia(hasta).fin;

        var comprobantes = await _db.Comprobantes.AsNoTracking()
            .Where(c => c.FechaEmision >= inicio && c.FechaEmision < fin)
            .OrderBy(c => c.FechaEmision)
            .ToListAsync();

        return AgruparPorDia(comprobantes);
    }

    // ── Reporte de Caja ──────────────────────────────────────────────────────

    public async Task<ReporteCajaDto> ReporteCajaAsync(string desde, string hasta)
    {
        var inicio = ParseDia(desde).inicio;
        var fin    = ParseDia(hasta).fin;

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
            .Where(c => c.FechaEmision >= inicio && c.FechaEmision < fin && c.Estado != "ANULADO" && c.Cobrado
                        && c.TipoComprobante != "NC") // la NC anula una venta anterior, no es ingreso nuevo
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
            Desde                     = desde,
            Hasta                     = hasta,
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

    public async Task<RegistroComprasDto> ReporteComprasAsync(string desde, string hasta)
    {
        var inicio = ParseDia(desde).inicio;
        var fin    = ParseDia(hasta).fin;

        var compras = await _db.Compras.AsNoTracking()
            .Include(c => c.Proveedor)
            .Where(c => c.FechaEmision >= inicio && c.FechaEmision < fin)
            .OrderBy(c => c.FechaEmision)
            .ToListAsync();

        var vigentes = compras.Where(c => c.Estado != "ANULADA").ToList();

        var detalle = compras.Select(c => new DetalleCompraReporteDto
        {
            Ruc             = c.Proveedor?.Ruc ?? string.Empty,
            RazonSocial     = c.Proveedor?.RazonSocial ?? c.NombreProveedorManual ?? string.Empty,
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
            Desde             = desde,
            Hasta             = hasta,
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

    public async Task<ReporteVentasCategoriaDto> ReporteVentasCategoriaAsync(string desde, string hasta)
    {
        var inicio = ParseDia(desde).inicio;
        var fin    = ParseDia(hasta).fin;

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
            Desde      = desde,
            Hasta      = hasta,
            MontoTotal = porCategoria.Sum(v => v.MontoTotal),
            Detalle    = porCategoria,
        };
    }

    // ── Productos más vendidos (todas las ambientes) ───────────────────────────

    public async Task<ReporteProductosMasVendidosDto> ReporteProductosMasVendidosAsync(string desde, string hasta)
    {
        var inicio = ParseDia(desde).inicio;
        var fin    = ParseDia(hasta).fin;

        var detalles = await _db.ComprobanteDetalles.AsNoTracking()
            .Include(d => d.Comprobante)
            .Where(d => d.Comprobante!.FechaEmision >= inicio && d.Comprobante.FechaEmision < fin &&
                        d.Comprobante.Estado != "ANULADO" && d.Comprobante.TipoComprobante != "NC")
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

        return new ReporteProductosMasVendidosDto { Desde = desde, Hasta = hasta, Detalle = porProducto };
    }

    // ── Utilidad (Comedor + Tienda) ─────────────────────────────────────────────

    private async Task<List<UtilidadDetalleDto>> ObtenerDetalleUtilidadAsync(DateTime inicio, DateTime fin)
    {
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
        // GenerarComprobanteTienda arma la descripción de cada línea. Se agrupa antes de
        // pasar a diccionario porque nada impide que existan dos productos con el mismo
        // nombre (solo el código de barras es único) — con dos iguales, ToDictionary
        // directo lanzaría ArgumentException por llave duplicada.
        var costosPorNombre = (await _db.Productos.AsNoTracking()
            .Select(p => new { p.Nombre, p.PrecioCompra })
            .ToListAsync())
            .GroupBy(p => p.Nombre)
            .ToDictionary(g => g.Key, g => g.First().PrecioCompra);

        var detallesTienda = await _db.ComprobanteDetalles.AsNoTracking()
            .Include(d => d.Comprobante)
            .Where(d => d.Comprobante!.TipoAmbiente == "tienda" &&
                        d.Comprobante.FechaEmision >= inicio && d.Comprobante.FechaEmision < fin &&
                        d.Comprobante.Estado != "ANULADO" && d.Comprobante.TipoComprobante != "NC")
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

        return filasComedor.Concat(filasTienda).OrderByDescending(f => f.Utilidad).ToList();
    }

    public async Task<ReporteUtilidadDto> ReporteUtilidadAsync(string desde, string hasta)
    {
        var inicio = ParseDia(desde).inicio;
        var fin    = ParseDia(hasta).fin;
        var todas = await ObtenerDetalleUtilidadAsync(inicio, fin);

        return new ReporteUtilidadDto
        {
            Desde         = desde,
            Hasta         = hasta,
            IngresoTotal  = todas.Sum(f => f.Ingreso),
            CostoTotal    = todas.Sum(f => f.Costo),
            UtilidadTotal = todas.Sum(f => f.Utilidad),
            Detalle       = todas,
        };
    }

    // ── Liquidación de Caja (resumen imprimible de un día específico) ───────────

    public async Task<LiquidacionCajaDto> ReporteLiquidacionCajaAsync(string fecha)
    {
        var (inicio, fin) = ParseDia(fecha);

        var itemsConCosto = await ObtenerDetalleUtilidadAsync(inicio, fin);

        // Baños y Habitaciones no tienen concepto de costo en el modelo, pero igual
        // cuentan para "todo lo que se vendió hoy" — aparecen con costo/utilidad en null.
        var detallesOtros = await _db.ComprobanteDetalles.AsNoTracking()
            .Include(d => d.Comprobante)
            .Where(d => (d.Comprobante!.TipoAmbiente == "banio" || d.Comprobante.TipoAmbiente == "habitacion") &&
                        d.Comprobante.FechaEmision >= inicio && d.Comprobante.FechaEmision < fin &&
                        d.Comprobante.Estado != "ANULADO" && d.Comprobante.TipoComprobante != "NC")
            .ToListAsync();

        var itemsSinCosto = detallesOtros.Select(d => new LiquidacionItemDto
        {
            Nombre          = d.Descripcion,
            Ambiente        = d.Comprobante!.TipoAmbiente,
            CantidadVendida = d.Cantidad,
            Ingreso         = d.Subtotal,
            Costo           = null,
            Utilidad        = null,
        });

        var items = itemsConCosto
            .Select(i => new LiquidacionItemDto
            {
                Nombre = i.Nombre, Ambiente = i.Ambiente, CantidadVendida = i.CantidadVendida,
                Ingreso = i.Ingreso, Costo = i.Costo, Utilidad = i.Utilidad,
            })
            .Concat(itemsSinCosto)
            .OrderByDescending(i => i.Ingreso)
            .ToList();

        var apertura = await _db.AperturasCaja.AsNoTracking().FirstOrDefaultAsync(a => a.Fecha >= inicio && a.Fecha < fin);
        var cierre   = await _db.CierresCaja.AsNoTracking().FirstOrDefaultAsync(c => c.Fecha >= inicio && c.Fecha < fin);
        var egresos  = await _db.EgresosCajaChica.AsNoTracking().Where(e => e.Fecha >= inicio && e.Fecha < fin).SumAsync(e => e.Monto);
        var ventasSistema = await _db.Comprobantes.AsNoTracking()
            .Where(c => c.FechaEmision >= inicio && c.FechaEmision < fin && c.Estado != "ANULADO" && c.Cobrado
                        && c.TipoComprobante != "NC")
            .SumAsync(c => c.Total);

        var efectivo      = cierre?.EfectivoFisico ?? 0;
        var yape          = cierre?.YapeFisico ?? 0;
        var transferencia = cierre?.TransferenciaFisico ?? 0;

        return new LiquidacionCajaDto
        {
            Fecha                = fecha,
            TieneApertura        = apertura is not null,
            MontoApertura        = apertura?.MontoInicial ?? 0,
            VentasSistema        = ventasSistema,
            EgresosCajaChica     = egresos,
            TieneCierre          = cierre is not null,
            EfectivoContado      = efectivo,
            YapeContado          = yape,
            TransferenciaContado = transferencia,
            TotalContado         = efectivo + yape + transferencia,
            Diferencia           = cierre?.Diferencia ?? 0,
            EstadoCaja           = cierre is not null ? "Cerrada" : apertura is not null ? "Abierta" : "Sin apertura",
            IngresoTotal         = items.Sum(i => i.Ingreso),
            CostoTotal           = itemsConCosto.Count > 0 ? itemsConCosto.Sum(i => i.Costo) : null,
            UtilidadTotal        = itemsConCosto.Count > 0 ? itemsConCosto.Sum(i => i.Utilidad) : null,
            Items                = items,
        };
    }

    // ── Ventas por cajero/mesero ─────────────────────────────────────────────

    public async Task<ReportePersonalDto> ReportePersonalAsync(string desde, string hasta)
    {
        var inicio = ParseDia(desde).inicio;
        var fin    = ParseDia(hasta).fin;

        var comprobantes = await _db.Comprobantes.AsNoTracking()
            .Where(c => c.FechaEmision >= inicio && c.FechaEmision < fin && c.Estado != "ANULADO"
                        && c.TipoComprobante != "NC")
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
            Desde      = desde,
            Hasta      = hasta,
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

    /// <summary>Acumulado de lo cobrado por Yape/Plin en un rango de fechas arbitrario.
    /// Incluye tanto comprobantes 100% QR como la porción QR de pagos Mixto — el reporte
    /// anterior (mensual, solo MetodoPago == YapePlin) subestimaba el total real porque
    /// ignoraba los pagos divididos efectivo + Yape.</summary>
    public async Task<ReportePagoQrDto> ReportePagoQrAsync(string desde, string hasta)
    {
        var inicio = ParseDia(desde).inicio;
        var fin    = ParseDia(hasta).fin;

        var comprobantes = await _db.Comprobantes.AsNoTracking()
            .Where(c => c.FechaEmision >= inicio && c.FechaEmision < fin &&
                        c.Estado != "ANULADO" && c.Cobrado && c.TipoComprobante != "NC" &&
                        (c.MetodoPago == MetodoPago.YapePlin || c.MetodoPago == MetodoPago.Mixto))
            .OrderBy(c => c.FechaEmision)
            .ToListAsync();

        var detalle = comprobantes.Select(c => new DetallePagoQrDto
        {
            NumeroFormateado = $"{c.Serie}-{c.Numero:D5}",
            TipoComprobante  = c.TipoComprobante,
            TipoAmbiente     = c.TipoAmbiente,
            ClienteNombre    = c.ClienteNombre ?? c.ClienteRazonSocial,
            MontoYape        = c.MetodoPago == MetodoPago.Mixto ? c.Total - (c.MontoEfectivoMixto ?? 0) : c.Total,
            EsMixto          = c.MetodoPago == MetodoPago.Mixto,
            FechaEmision     = c.FechaEmision,
        }).ToList();

        return new ReportePagoQrDto
        {
            Desde              = desde,
            Hasta              = hasta,
            TotalTransacciones = detalle.Count,
            MontoTotal         = detalle.Sum(d => d.MontoYape),
            Detalle            = detalle,
        };
    }

    // ── Reporte de comandas ───────────────────────────────────────────────────

    public async Task<ReporteComandasDto> ReporteComandasAsync(string desde, string hasta)
    {
        var inicio = ParseDia(desde).inicio;
        var fin    = ParseDia(hasta).fin;

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
            Desde                 = desde,
            Hasta                 = hasta,
            TotalComandas         = detalle.Count,
            TiempoPromedioMinutos = duraciones.Count > 0 ? Math.Round(duraciones.Average(), 1) : 0,
            Detalle               = detalle,
        };
    }

    // ── Stock mínimo ──────────────────────────────────────────────────────────

    public async Task<ReporteStockMinimoDto> ReporteStockMinimoAsync()
    {
        var insumos = await _db.Insumos.AsNoTracking()
            .Where(i => i.Activo && i.StockMinimo > 0 && i.StockActual <= i.StockMinimo)
            .Select(i => new StockBajoDto { Nombre = i.Nombre, Unidad = i.Unidad, StockActual = i.StockActual, StockMinimo = i.StockMinimo })
            .ToListAsync();

        var productos = await _db.Productos.AsNoTracking()
            .Where(p => p.Activo && p.StockMinimo > 0 && p.Stock <= p.StockMinimo)
            .Select(p => new StockBajoDto { Nombre = p.Nombre, Unidad = null, StockActual = p.Stock, StockMinimo = p.StockMinimo })
            .ToListAsync();

        return new ReporteStockMinimoDto { Insumos = insumos, Productos = productos };
    }
}
