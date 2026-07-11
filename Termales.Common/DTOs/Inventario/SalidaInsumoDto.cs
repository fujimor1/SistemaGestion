namespace Termales.Common.DTOs.Inventario;

public class SalidaInsumoDto
{
    public int SalidaInsumoId { get; set; }
    public int InsumoId { get; set; }
    public string NombreInsumo { get; set; } = string.Empty;
    public string? Unidad { get; set; }
    public decimal Cantidad { get; set; }
    public DateTime Fecha { get; set; }
    public string? Observacion { get; set; }
}

public class RegistrarSalidaInsumoDto
{
    public int InsumoId { get; set; }
    public decimal Cantidad { get; set; }
    public string? Observacion { get; set; }
}
