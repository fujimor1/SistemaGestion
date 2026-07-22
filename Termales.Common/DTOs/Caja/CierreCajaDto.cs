namespace Termales.Common.DTOs.Caja;

public class ResumenAmbienteDto
{
    public string Ambiente { get; set; } = string.Empty;
    public string NombreAmbiente { get; set; } = string.Empty;
    public int CantidadTransacciones { get; set; }
    public decimal Total { get; set; }
}

public class DatosCierreDto
{
    public decimal TotalSistema { get; set; }
    // Cuánto de TotalSistema corresponde a cada método de pago (Mixto se reparte según
    // MontoEfectivoMixto) — para comparar contra el conteo físico método por método antes
    // de cerrar, y no solo el total agregado.
    public decimal EfectivoSistema { get; set; }
    public decimal YapeSistema { get; set; }
    public decimal MontoApertura { get; set; }
    public decimal TotalEgresos { get; set; }
    public decimal SaldoCajaChica { get; set; }
    // Lo que debería haber físicamente en caja considerando que los egresos salen de
    // ahí mismo: apertura + lo cobrado - lo pagado en egresos. Antes el conteo físico
    // se comparaba contra EfectivoSistema/TotalSistema directo (sin restar egresos ni
    // sumar la apertura), así que un cierre sin diferencias no detectaba que faltaba
    // exactamente el monto de los egresos.
    public decimal EfectivoEsperado { get; set; }
    public decimal TotalEsperado { get; set; }
    public List<ResumenAmbienteDto> ResumenPorAmbiente { get; set; } = new();
    public CierreCajaDto? CierreExistente { get; set; }
}

public class CierreCajaDto
{
    public int CierreCajaId { get; set; }
    public DateTime Fecha { get; set; }
    public decimal TotalSistema { get; set; }
    public decimal EfectivoSistema { get; set; }
    public decimal YapeSistema { get; set; }
    public decimal EfectivoFisico { get; set; }
    public decimal YapeFisico { get; set; }
    public decimal TransferenciaFisico { get; set; }
    public decimal TotalEgresos { get; set; }
    public decimal MontoApertura { get; set; }
    public decimal Diferencia { get; set; }
    /// <summary>Efectivo físico que el encargado deja en caja para la apertura del día siguiente.</summary>
    public decimal MontoDejado { get; set; }
    public string? Observaciones { get; set; }
    public string EncargadoCierre { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; }
}

public class CerrarCajaDto
{
    public decimal EfectivoFisico { get; set; }
    public decimal YapeFisico { get; set; }
    public decimal TransferenciaFisico { get; set; }
    /// <summary>Efectivo físico que el encargado deja en caja para la apertura del día siguiente.</summary>
    public decimal MontoDejado { get; set; }
    public string? Observaciones { get; set; }
    public string EncargadoCierre { get; set; } = string.Empty;
}
