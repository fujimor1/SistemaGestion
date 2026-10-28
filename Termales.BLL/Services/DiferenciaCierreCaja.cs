using Termales.Entities.Models.Caja;

namespace Termales.BLL.Services;

/// <summary>Reparte la Diferencia de un cierre de caja entre efectivo y Yape/Plin. Con signo:
/// + sobrante, - faltante. La suma de ambas es igual a CierreCaja.Diferencia.</summary>
public static class DiferenciaCierreCaja
{
    /// <summary>Null/null si no hay cierre, o si es un cierre viejo anterior al desglose del
    /// sistema por método (EfectivoSistema/YapeSistema en 0 pese a haber ventas): ahí no se
    /// puede repartir la diferencia.</summary>
    public static (decimal? Efectivo, decimal? Yape) Calcular(CierreCaja? cierre)
    {
        if (cierre is null) return (null, null);

        var tieneDesglose = cierre.EfectivoSistema != 0 || cierre.YapeSistema != 0 || cierre.TotalSistema == 0;
        if (!tieneDesglose) return (null, null);

        // Los egresos salen físicamente del efectivo; la apertura ya estaba en caja.
        var efectivo = cierre.EfectivoFisico - (cierre.MontoApertura + cierre.EfectivoSistema - cierre.TotalEgresos);
        // La transferencia contada se compara del lado digital, igual que en el cierre.
        var yape = cierre.YapeFisico + cierre.TransferenciaFisico - cierre.YapeSistema;
        return (efectivo, yape);
    }
}
