namespace Termales.Entities.Models.Inventario;

public class SalidaInsumo
{
    public int SalidaInsumoId { get; set; }
    public int InsumoId { get; set; }
    public decimal Cantidad { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public string? Observacion { get; set; }
    public Insumo Insumo { get; set; } = null!;
}
