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
}
