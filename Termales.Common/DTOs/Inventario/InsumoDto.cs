namespace Termales.Common.DTOs.Inventario;

public class InsumoDto
{
    public int InsumoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string TipoAmbiente { get; set; } = string.Empty;
    public string TipoArticulo { get; set; } = "insumo";
    public string? Unidad { get; set; }
    public decimal StockActual { get; set; }
    public decimal StockMinimo { get; set; }
    public decimal PrecioReferencia { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaRegistro { get; set; }
}

public class CrearInsumoDto
{
    public string Nombre { get; set; } = string.Empty;
    public string TipoAmbiente { get; set; } = string.Empty;
    public string TipoArticulo { get; set; } = "insumo";
    public string? Unidad { get; set; }
    public decimal StockActual { get; set; }
    public decimal StockMinimo { get; set; }
    public decimal PrecioReferencia { get; set; }
}

public class ActualizarInsumoDto
{
    public string Nombre { get; set; } = string.Empty;
    public string? Unidad { get; set; }
    public decimal StockMinimo { get; set; }
    public decimal PrecioReferencia { get; set; }
    public bool Activo { get; set; }
}
