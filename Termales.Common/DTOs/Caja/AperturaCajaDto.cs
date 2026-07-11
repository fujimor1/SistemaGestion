namespace Termales.Common.DTOs.Caja;

public class AperturaCajaDto
{
    public int AperturaCajaId { get; set; }
    public DateTime Fecha { get; set; }
    public decimal MontoInicial { get; set; }
    public string Responsable { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
}

public class AbrirCajaDto
{
    public decimal MontoInicial { get; set; }
    public string Responsable { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
}
