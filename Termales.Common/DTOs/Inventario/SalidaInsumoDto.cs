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

/// <summary>Consumo actual: registra la salida de varios insumos a la vez para
/// un ambiente y emite un ticket de referencia con el detalle.</summary>
public class RegistrarConsumoActualDto
{
    public string Ambiente { get; set; } = string.Empty;
    public List<ItemConsumoActualDto> Items { get; set; } = new();
}

public class ItemConsumoActualDto
{
    public int InsumoId { get; set; }
    public decimal Cantidad { get; set; }
}
