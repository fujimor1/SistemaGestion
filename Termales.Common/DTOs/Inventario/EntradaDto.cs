namespace Termales.Common.DTOs.Inventario;

public class EntradaInsumoDto
{
    public int EntradaInsumoId { get; set; }
    public int InsumoId { get; set; }
    public string NombreInsumo { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Total { get; set; }
    public DateTime Fecha { get; set; }
    public string? Observacion { get; set; }
}

public class RegistrarEntradaInsumoDto
{
    public int InsumoId { get; set; }
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public string? Observacion { get; set; }
}

public class EntradaProductoDto
{
    public int EntradaProductoId { get; set; }
    public int ProductoId { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Total { get; set; }
    public DateTime Fecha { get; set; }
    public string? Observacion { get; set; }
}

public class RegistrarEntradaProductoDto
{
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public string? Observacion { get; set; }
}
