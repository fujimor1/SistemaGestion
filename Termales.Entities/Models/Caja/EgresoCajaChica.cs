namespace Termales.Entities.Models.Caja;

public class EgresoCajaChica
{
    public int EgresoCajaChicaId { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public string Concepto { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public string Responsable { get; set; } = string.Empty;
    public string? TipoDocumento { get; set; }
    public string? NumeroDocumento { get; set; }
    public string RegistradoPor { get; set; } = string.Empty;
    public string? Observaciones { get; set; }

    // No nulo únicamente cuando el egreso se generó automáticamente al pagar
    // una Compra con "Pagar con Caja Chica" — permite distinguirlo de un
    // egreso manual sin depender de parsear el texto de Concepto.
    public int? CompraId { get; set; }
}
