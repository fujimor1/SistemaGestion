namespace Termales.Common.DTOs.Inventario;

public class SalidaProductoDto
{
    public int SalidaProductoId { get; set; }
    public int ProductoId { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public DateTime Fecha { get; set; }
    public string? Observacion { get; set; }
}

public class RegistrarSalidaProductoDto
{
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }
    public string? Observacion { get; set; }
}
