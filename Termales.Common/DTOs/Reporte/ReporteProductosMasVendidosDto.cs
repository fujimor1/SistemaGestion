namespace Termales.Common.DTOs.Reporte;

public class ReporteProductosMasVendidosDto
{
    public string Desde { get; set; } = string.Empty;
    public string Hasta { get; set; } = string.Empty;
    public List<AmbienteProductosMasVendidosDto> Ambientes { get; set; } = [];
    public decimal MontoTotalGeneral { get; set; }
}

public class AmbienteProductosMasVendidosDto
{
    public string Ambiente { get; set; } = string.Empty;
    public List<ProductoMasVendidoDto> Productos { get; set; } = [];
    /// <summary>Repartido según la forma de pago real de cada comprobante de este ambiente
    /// (Mixto se reparte entre efectivo y yape según MontoEfectivoMixto).</summary>
    public decimal MontoEfectivo { get; set; }
    public decimal MontoYape { get; set; }
    public decimal MontoTotal { get; set; }
}

public class ProductoMasVendidoDto
{
    public string Descripcion { get; set; } = string.Empty;
    /// <summary>Precio promedio (MontoTotal / CantidadVendida) — evita inconsistencias si el
    /// precio cambió dentro del rango de fechas.</summary>
    public decimal PrecioUnitario { get; set; }
    public int CantidadVendida { get; set; }
    public decimal MontoTotal { get; set; }
}
