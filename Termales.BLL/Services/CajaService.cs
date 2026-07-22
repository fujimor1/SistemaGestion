using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using Termales.BLL.Interfaces;
using Termales.Common.DTOs.Caja;
using Termales.DAL.Context;
using Termales.Entities.Enums;
using Termales.Entities.Models.Caja;

namespace Termales.BLL.Services;

public class CajaService : ICajaService
{
    private readonly TermalesDbContext _db;

    public CajaService(TermalesDbContext db) => _db = db;

    private static readonly Dictionary<string, string> NombresAmbiente = new()
    {
        ["comedor"]     = "Comedor",
        ["banio"]       = "Baños Termales",
        ["habitacion"]  = "Hospedaje",
        ["tienda"]      = "Tienda",
    };

    // Perú es UTC-5 fijo (sin horario de verano): medianoche en Lima = 05:00 UTC del
    // mismo día. Antes todo este archivo usaba DateTime.UtcNow.Date directo, así que
    // entre las 7pm y medianoche hora Perú (cuando UTC ya cruzó al día siguiente) el
    // sistema creía que ya era "mañana": no encontraba la apertura de hoy, dejaba
    // registrar un cierre para el día equivocado, y al día siguiente la caja aparecía
    // "ya cerrada" sin que nadie la hubiera cerrado en ese día real — un cierre
    // "fantasma" que explica que la caja se vea cerrada sin que el cajero la cerrara.
    private static readonly TimeSpan OffsetPeru = TimeSpan.FromHours(5);

    // Día de negocio "hoy" en Perú, para las claves de Apertura/Cierre (que solo se
    // comparan por fecha, sin hora).
    private static DateTime HoyPeru() => (DateTime.UtcNow - OffsetPeru).Date;

    // Rango [inicio, fin) en UTC de un día de negocio en Perú, para filtrar
    // timestamps reales (ej. EgresoCajaChica.Fecha, Comprobante.FechaEmision) en vez
    // de compararlos por fecha directa.
    private static (DateTime inicio, DateTime fin) RangoDiaPeru(DateTime dia)
    {
        var inicio = dia.Date + OffsetPeru;
        return (inicio, inicio.AddDays(1));
    }

    // ── Apertura ──────────────────────────────────────────────────────────────

    public async Task<AperturaCajaDto?> ObtenerAperturaHoyAsync()
    {
        var hoy = HoyPeru();
        var apertura = await _db.AperturasCaja.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Fecha.Date == hoy);
        return apertura is null ? null : MapApertura(apertura);
    }

    public async Task<AperturaCajaDto> AbrirCajaAsync(AbrirCajaDto dto, string registradoPor)
    {
        var hoy = HoyPeru();
        var existente = await _db.AperturasCaja.FirstOrDefaultAsync(a => a.Fecha.Date == hoy);
        if (existente is not null)
            throw new InvalidOperationException("La caja ya fue abierta hoy.");

        var apertura = new AperturaCaja
        {
            Fecha = hoy,
            MontoInicial = dto.MontoInicial,
            Responsable = dto.Responsable,
            Observaciones = dto.Observaciones,
        };
        _db.AperturasCaja.Add(apertura);
        await _db.SaveChangesAsync();
        return MapApertura(apertura);
    }

    public async Task<bool> HayCajaAbiertaAsync()
    {
        var hoy = HoyPeru();
        var hayApertura = await _db.AperturasCaja.AsNoTracking().AnyAsync(a => a.Fecha.Date == hoy);
        if (!hayApertura) return false;

        var yaCerrada = await _db.CierresCaja.AsNoTracking().AnyAsync(c => c.Fecha.Date == hoy);
        return !yaCerrada;
    }

    // ── Egresos ───────────────────────────────────────────────────────────────

    public async Task<IEnumerable<EgresoCajaChicaDto>> ObtenerEgresosHoyAsync()
    {
        return await ObtenerEgresosPorFechaAsync(HoyPeru());
    }

    public async Task<IEnumerable<EgresoCajaChicaDto>> ObtenerEgresosPorFechaAsync(DateTime fecha)
    {
        var (inicio, fin) = RangoDiaPeru(fecha);
        var egresos = await _db.EgresosCajaChica.AsNoTracking()
            .Where(e => e.Fecha >= inicio && e.Fecha < fin)
            .OrderByDescending(e => e.Fecha)
            .ToListAsync();
        return egresos.Select(MapEgreso);
    }

    public async Task<EgresoCajaChicaDto> RegistrarEgresoAsync(RegistrarEgresoDto dto, string registradoPor, int? compraId = null)
    {
        var egreso = new EgresoCajaChica
        {
            Concepto = dto.Concepto,
            Monto = dto.Monto,
            Responsable = registradoPor,
            TipoDocumento = dto.TipoDocumento,
            NumeroDocumento = dto.NumeroDocumento,
            RegistradoPor = registradoPor,
            Observaciones = dto.Observaciones,
            CompraId = compraId,
        };
        _db.EgresosCajaChica.Add(egreso);
        await _db.SaveChangesAsync();
        return MapEgreso(egreso);
    }

    public async Task<bool> EliminarEgresoAsync(int id)
    {
        var egreso = await _db.EgresosCajaChica.FindAsync(id);
        if (egreso is null) return false;
        _db.EgresosCajaChica.Remove(egreso);
        await _db.SaveChangesAsync();
        return true;
    }

    // ── Cierre ────────────────────────────────────────────────────────────────

    // Una venta cuenta para el día en que el dinero realmente entró a caja: para un
    // cobro directo (Efectivo/Yape/Mixto) eso es FechaEmision, pero para una deuda a
    // Crédito que se cobra después, es FechaCobro (el día de la venta original puede
    // ser otro). El total general se reparte entre Efectivo y Yape/Plin según el método
    // real de cada comprobante (Mixto se divide según MontoEfectivoMixto); Transferencia
    // (ya no seleccionable, solo dato histórico) no cae en ninguno de los dos, pero sí
    // suma al total general.
    private async Task<(decimal Efectivo, decimal YapePlin, decimal TotalGeneral)> ObtenerTotalesPorMetodoAsync(DateTime dia)
    {
        var (inicio, fin) = RangoDiaPeru(dia);
        var comprobantes = await _db.Comprobantes.AsNoTracking()
            .Where(c => c.Estado != "ANULADO" && c.Cobrado && c.TipoComprobante != "NC"
                        && (c.FechaCobro ?? c.FechaEmision) >= inicio && (c.FechaCobro ?? c.FechaEmision) < fin)
            .Select(c => new { c.MetodoPago, c.Total, c.MontoEfectivoMixto })
            .ToListAsync();

        decimal efectivo = 0, yape = 0;
        foreach (var c in comprobantes)
        {
            switch (c.MetodoPago)
            {
                case MetodoPago.Efectivo:
                    efectivo += c.Total;
                    break;
                case MetodoPago.YapePlin:
                    yape += c.Total;
                    break;
                case MetodoPago.Mixto:
                    var montoEfectivo = c.MontoEfectivoMixto ?? 0;
                    efectivo += montoEfectivo;
                    yape += c.Total - montoEfectivo;
                    break;
            }
        }

        return (efectivo, yape, comprobantes.Sum(c => c.Total));
    }

    public async Task<DatosCierreDto> ObtenerDatosCierreAsync()
    {
        var hoy = HoyPeru();
        var (inicio, fin) = RangoDiaPeru(hoy);

        var (efectivoSistema, yapeSistema, totalSistema) = await ObtenerTotalesPorMetodoAsync(hoy);

        var apertura = await _db.AperturasCaja.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Fecha.Date == hoy);

        var totalEgresos = await _db.EgresosCajaChica.AsNoTracking()
            .Where(e => e.Fecha >= inicio && e.Fecha < fin)
            .SumAsync(e => (decimal?)e.Monto) ?? 0;

        var resumenRaw = await _db.Comprobantes.AsNoTracking()
            .Where(c => (c.FechaCobro ?? c.FechaEmision) >= inicio && (c.FechaCobro ?? c.FechaEmision) < fin
                        && c.Estado != "ANULADO" && c.Cobrado && c.TipoComprobante != "NC")
            .GroupBy(c => c.TipoAmbiente)
            .Select(g => new { Ambiente = g.Key, Cantidad = g.Count(), Total = g.Sum(c => c.Total) })
            .ToListAsync();

        var resumen = resumenRaw.Select(x => new ResumenAmbienteDto
        {
            Ambiente = x.Ambiente,
            NombreAmbiente = NombresAmbiente.GetValueOrDefault(x.Ambiente, x.Ambiente),
            CantidadTransacciones = x.Cantidad,
            Total = x.Total,
        }).OrderBy(r => r.NombreAmbiente).ToList();

        var cierreExistente = await _db.CierresCaja.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Fecha.Date == hoy);

        var montoApertura = apertura?.MontoInicial ?? 0;
        // Los egresos salen físicamente de la caja — sin restarlos (y sin sumar la
        // apertura), un cierre "sin diferencias" no detectaba que faltaba justo el
        // monto de los egresos del día.
        var efectivoEsperado = montoApertura + efectivoSistema - totalEgresos;

        return new DatosCierreDto
        {
            TotalSistema = totalSistema,
            EfectivoSistema = efectivoSistema,
            YapeSistema = yapeSistema,
            MontoApertura = montoApertura,
            TotalEgresos = totalEgresos,
            SaldoCajaChica = montoApertura - totalEgresos,
            EfectivoEsperado = efectivoEsperado,
            TotalEsperado = efectivoEsperado + yapeSistema,
            ResumenPorAmbiente = resumen,
            CierreExistente = cierreExistente is null ? null : MapCierre(cierreExistente),
        };
    }

    public async Task<CierreCajaDto> CerrarCajaAsync(CerrarCajaDto dto)
    {
        var hoy = HoyPeru();
        var (inicio, fin) = RangoDiaPeru(hoy);

        var existente = await _db.CierresCaja.FirstOrDefaultAsync(c => c.Fecha.Date == hoy);
        if (existente is not null)
            throw new InvalidOperationException("La caja ya fue cerrada hoy.");

        var (efectivoSistema, yapeSistema, totalSistema) = await ObtenerTotalesPorMetodoAsync(hoy);

        var apertura = await _db.AperturasCaja.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Fecha.Date == hoy);

        var totalEgresos = await _db.EgresosCajaChica.AsNoTracking()
            .Where(e => e.Fecha >= inicio && e.Fecha < fin)
            .SumAsync(e => (decimal?)e.Monto) ?? 0;

        var montoApertura = apertura?.MontoInicial ?? 0;
        // Igual que en ObtenerDatosCierreAsync: los egresos salen físicamente de la
        // caja, así que la diferencia se calcula contra lo que debería haber
        // (apertura + ventas - egresos), no contra las ventas crudas.
        var totalEsperado = montoApertura + totalSistema - totalEgresos;
        var totalFisico = dto.EfectivoFisico + dto.YapeFisico + dto.TransferenciaFisico;
        var diferencia = totalFisico - totalEsperado;

        var cierre = new CierreCaja
        {
            Fecha = hoy,
            TotalSistema = totalSistema,
            EfectivoSistema = efectivoSistema,
            YapeSistema = yapeSistema,
            EfectivoFisico = dto.EfectivoFisico,
            YapeFisico = dto.YapeFisico,
            TransferenciaFisico = dto.TransferenciaFisico,
            TotalEgresos = totalEgresos,
            MontoApertura = montoApertura,
            Diferencia = diferencia,
            Observaciones = dto.Observaciones,
            EncargadoCierre = dto.EncargadoCierre,
        };

        _db.CierresCaja.Add(cierre);
        await _db.SaveChangesAsync();
        return MapCierre(cierre);
    }

    // ── Mappers ───────────────────────────────────────────────────────────────

    private static AperturaCajaDto MapApertura(AperturaCaja a) => new()
    {
        AperturaCajaId = a.AperturaCajaId,
        Fecha = a.Fecha,
        MontoInicial = a.MontoInicial,
        Responsable = a.Responsable,
        Observaciones = a.Observaciones,
    };

    private static EgresoCajaChicaDto MapEgreso(EgresoCajaChica e) => new()
    {
        EgresoCajaChicaId = e.EgresoCajaChicaId,
        Fecha = e.Fecha,
        Concepto = e.Concepto,
        Monto = e.Monto,
        Responsable = e.Responsable,
        TipoDocumento = e.TipoDocumento,
        NumeroDocumento = e.NumeroDocumento,
        RegistradoPor = e.RegistradoPor,
        Observaciones = e.Observaciones,
        CompraId = e.CompraId,
    };

    private static CierreCajaDto MapCierre(CierreCaja c) => new()
    {
        CierreCajaId = c.CierreCajaId,
        Fecha = c.Fecha,
        TotalSistema = c.TotalSistema,
        EfectivoSistema = c.EfectivoSistema,
        YapeSistema = c.YapeSistema,
        EfectivoFisico = c.EfectivoFisico,
        YapeFisico = c.YapeFisico,
        TransferenciaFisico = c.TransferenciaFisico,
        TotalEgresos = c.TotalEgresos,
        MontoApertura = c.MontoApertura,
        Diferencia = c.Diferencia,
        Observaciones = c.Observaciones,
        EncargadoCierre = c.EncargadoCierre,
        FechaRegistro = c.FechaRegistro,
    };
}
