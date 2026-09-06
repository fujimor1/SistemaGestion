namespace Termales.Common.Utils;

/// <summary>
/// Perú es UTC-5 fijo (sin horario de verano): medianoche en Lima = 05:00 UTC del
/// mismo día. Código que use <c>DateTime.UtcNow.Date</c> o <c>.Date ==</c> directo
/// cree que ya es "mañana" entre las 7pm y medianoche hora Perú, mientras UTC ya
/// cruzó al día siguiente. Usar siempre estos helpers en vez de reimplementar el
/// offset (-5h) por archivo.
/// </summary>
public static class PeruTime
{
    private static readonly TimeSpan OffsetPeru = TimeSpan.FromHours(5);

    /// <summary>Instante actual ajustado a hora Perú (para timestamps que se guardan o comparan como si fueran locales).</summary>
    public static DateTime Now() => DateTime.SpecifyKind(DateTime.UtcNow - OffsetPeru, DateTimeKind.Utc);

    /// <summary>Día de negocio "hoy" en Perú, para claves que solo se comparan por fecha (sin hora).</summary>
    public static DateTime Today() => Now().Date;

    /// <summary>Rango [inicio, fin) en UTC de un día de negocio en Perú, para filtrar timestamps reales (ej. FechaCobro, FechaVenta) en vez de compararlos por fecha directa.</summary>
    public static (DateTime inicio, DateTime fin) DayRange(DateTime dia)
    {
        // Se fuerza Kind=Utc explícitamente: si `dia` viene de un `new DateTime(y,m,1)`
        // (Kind=Unspecified) en vez de Today()/Now(), Npgsql rechaza el parámetro contra
        // una columna `timestamp with time zone` ("only UTC is supported").
        var inicio = DateTime.SpecifyKind(dia.Date, DateTimeKind.Utc) + OffsetPeru;
        return (inicio, inicio.AddDays(1));
    }

    /// <summary>Rango [inicio, fin) en UTC de un día de negocio en Perú, a partir de un <see cref="DateOnly"/>.</summary>
    public static (DateTime inicio, DateTime fin) DayRange(DateOnly dia) => DayRange(dia.ToDateTime(TimeOnly.MinValue));

    /// <summary>Día de negocio en Perú al que pertenece un instante real (ej. Comprobante.FechaVenta,
    /// EgresoCajaChica.Fecha, ambos guardados como DateTime.UtcNow). Usar para AGRUPAR por día — un
    /// simple <c>DateOnly.FromDateTime(instante)</c> toma la fecha calendario UTC cruda, que entre las
    /// 7pm y medianoche hora Perú ya es "mañana" en UTC y mete la venta en el día equivocado.
    /// NO usar sobre AperturaCaja.Fecha/CierreCaja.Fecha/Compra.FechaEmision: esas son una clave de
    /// día (medianoche, sin hora real), no un instante — ya están en "día Perú" y no necesitan este
    /// ajuste (aplicarlo las correría un día para atrás).</summary>
    public static DateOnly BusinessDay(DateTime instante) => DateOnly.FromDateTime(instante - OffsetPeru);
}
