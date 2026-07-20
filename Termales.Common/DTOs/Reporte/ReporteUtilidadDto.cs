namespace Termales.Common.DTOs.Reporte;

public class ReporteUtilidadDto
{
    public string Desde { get; set; } = string.Empty;
    public string Hasta { get; set; } = string.Empty;
    public decimal IngresoTotal { get; set; }
    public decimal CostoTotal { get; set; }
    public decimal UtilidadTotal { get; set; }
    /// <summary>Solo Comedor y Tienda tienen costo conocido (receta / precio de compra). Baños y
    /// Habitación no tienen concepto de costo en el modelo, así que no aparecen aquí.</summary>
    public List<UtilidadDetalleDto> Detalle { get; set; } = [];
}

public class UtilidadDetalleDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Ambiente { get; set; } = string.Empty;
    public int CantidadVendida { get; set; }
    public decimal Ingreso { get; set; }
    public decimal Costo { get; set; }
    public decimal Utilidad { get; set; }
}
