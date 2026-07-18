namespace Termales.Common.DTOs.Caja;

public class EgresoCajaChicaDto
{
    public int EgresoCajaChicaId { get; set; }
    public DateTime Fecha { get; set; }
    public string Concepto { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public string Responsable { get; set; } = string.Empty;
    public string? TipoDocumento { get; set; }
    public string? NumeroDocumento { get; set; }
    public string RegistradoPor { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
    public int? CompraId { get; set; }
}

public class RegistrarEgresoDto
{
    public string Concepto { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public string? TipoDocumento { get; set; }
    public string? NumeroDocumento { get; set; }
    public string? Observaciones { get; set; }
}
