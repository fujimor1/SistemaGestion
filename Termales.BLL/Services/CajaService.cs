using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using Termales.BLL.Interfaces;
using Termales.Common.DTOs.Caja;
using Termales.DAL.Context;
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

    // ── Apertura ──────────────────────────────────────────────────────────────

    public async Task<AperturaCajaDto?> ObtenerAperturaHoyAsync()
    {
        var hoy = DateTime.UtcNow.Date;
        var apertura = await _db.AperturasCaja.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Fecha.Date == hoy);
        return apertura is null ? null : MapApertura(apertura);
    }

    public async Task<AperturaCajaDto> AbrirCajaAsync(AbrirCajaDto dto, string registradoPor)
    {
        var hoy = DateTime.UtcNow.Date;
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
        var hoy = DateTime.UtcNow.Date;
        var hayApertura = await _db.AperturasCaja.AsNoTracking().AnyAsync(a => a.Fecha.Date == hoy);
        if (!hayApertura) return false;

        var yaCerrada = await _db.CierresCaja.AsNoTracking().AnyAsync(c => c.Fecha.Date == hoy);
        return !yaCerrada;
    }

    // ── Egresos ───────────────────────────────────────────────────────────────

    public async Task<IEnumerable<EgresoCajaChicaDto>> ObtenerEgresosHoyAsync()
    {
        var hoy = DateTime.UtcNow.Date;
        return await ObtenerEgresosPorFechaAsync(hoy);
    }

    public async Task<IEnumerable<EgresoCajaChicaDto>> ObtenerEgresosPorFechaAsync(DateTime fecha)
    {
        var egresos = await _db.EgresosCajaChica.AsNoTracking()
            .Where(e => e.Fecha.Date == fecha.Date)
            .OrderByDescending(e => e.Fecha)
            .ToListAsync();
        return egresos.Select(MapEgreso);
    }

    public async Task<EgresoCajaChicaDto> RegistrarEgresoAsync(RegistrarEgresoDto dto, string registradoPor)
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

    public async Task<DatosCierreDto> ObtenerDatosCierreAsync()
    {
        var hoy = DateTime.UtcNow.Date;

        var totalSistema = await _db.Comprobantes.AsNoTracking()
            .Where(c => c.FechaEmision.Date == hoy && c.Estado != "ANULADO" && c.Cobrado)
            .SumAsync(c => (decimal?)c.Total) ?? 0;

        var apertura = await _db.AperturasCaja.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Fecha.Date == hoy);

        var totalEgresos = await _db.EgresosCajaChica.AsNoTracking()
            .Where(e => e.Fecha.Date == hoy)
            .SumAsync(e => (decimal?)e.Monto) ?? 0;

        var resumenRaw = await _db.Comprobantes.AsNoTracking()
            .Where(c => c.FechaEmision.Date == hoy && c.Estado != "ANULADO" && c.Cobrado)
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

        return new DatosCierreDto
        {
            TotalSistema = totalSistema,
            MontoApertura = apertura?.MontoInicial ?? 0,
            TotalEgresos = totalEgresos,
            SaldoCajaChica = (apertura?.MontoInicial ?? 0) - totalEgresos,
            ResumenPorAmbiente = resumen,
            CierreExistente = cierreExistente is null ? null : MapCierre(cierreExistente),
        };
    }

    public async Task<CierreCajaDto> CerrarCajaAsync(CerrarCajaDto dto)
    {
        var hoy = DateTime.UtcNow.Date;

        var existente = await _db.CierresCaja.FirstOrDefaultAsync(c => c.Fecha.Date == hoy);
        if (existente is not null)
            throw new InvalidOperationException("La caja ya fue cerrada hoy.");

        var totalSistema = await _db.Comprobantes.AsNoTracking()
            .Where(c => c.FechaEmision.Date == hoy && c.Estado != "ANULADO" && c.Cobrado)
            .SumAsync(c => (decimal?)c.Total) ?? 0;

        var apertura = await _db.AperturasCaja.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Fecha.Date == hoy);

        var totalEgresos = await _db.EgresosCajaChica.AsNoTracking()
            .Where(e => e.Fecha.Date == hoy)
            .SumAsync(e => (decimal?)e.Monto) ?? 0;

        var totalFisico = dto.EfectivoFisico + dto.YapeFisico + dto.TransferenciaFisico;
        var diferencia = totalFisico - totalSistema;

        var cierre = new CierreCaja
        {
            Fecha = hoy,
            TotalSistema = totalSistema,
            EfectivoFisico = dto.EfectivoFisico,
            YapeFisico = dto.YapeFisico,
            TransferenciaFisico = dto.TransferenciaFisico,
            TotalEgresos = totalEgresos,
            MontoApertura = apertura?.MontoInicial ?? 0,
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
    };

    private static CierreCajaDto MapCierre(CierreCaja c) => new()
    {
        CierreCajaId = c.CierreCajaId,
        Fecha = c.Fecha,
        TotalSistema = c.TotalSistema,
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
