namespace Termales.Entities.Models.Caja;

public class CierreCaja
{
    public int CierreCajaId { get; set; }
    public DateTime Fecha { get; set; }
    // Sistema
    public decimal TotalSistema { get; set; }
    // Desglose del sistema por método de pago (Mixto se reparte entre estos dos según
    // MontoEfectivoMixto) — para poder comparar contra el conteo físico método por método.
    public decimal EfectivoSistema { get; set; }
    public decimal YapeSistema { get; set; }
    // Conteo físico (ingresado por el cajero)
    public decimal EfectivoFisico { get; set; }
    public decimal YapeFisico { get; set; }
    public decimal TransferenciaFisico { get; set; }
    // Caja chica
    public decimal TotalEgresos { get; set; }
    public decimal MontoApertura { get; set; }
    // Resultado
    public decimal Diferencia { get; set; }
    // Cuánto efectivo físico deja el encargado en la caja (para la apertura del
    // día siguiente) — el resto de lo contado se retira/deposita aparte.
    public decimal MontoDejado { get; set; }
    // Meta
    public string? Observaciones { get; set; }
    public string EncargadoCierre { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
}
