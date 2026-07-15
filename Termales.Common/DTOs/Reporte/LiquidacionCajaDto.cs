namespace Termales.Common.DTOs.Reporte;

/// <summary>Resumen imprimible de un día: todo lo vendido (con costo cuando se conoce) más el
/// cuadre de caja del mismo día, sin importar la forma de pago.</summary>
public class LiquidacionCajaDto
{
    public string Fecha { get; set; } = string.Empty;

    public bool TieneApertura { get; set; }
    public decimal MontoApertura { get; set; }
    public decimal VentasSistema { get; set; }
    public decimal EgresosCajaChica { get; set; }
    public bool TieneCierre { get; set; }
    public decimal EfectivoContado { get; set; }
    public decimal YapeContado { get; set; }
    public decimal TransferenciaContado { get; set; }
    public decimal TotalContado { get; set; }
    public decimal Diferencia { get; set; }
    public string EstadoCaja { get; set; } = string.Empty;

    public decimal IngresoTotal { get; set; }
    /// <summary>Null si ningún ítem del día tiene costo conocido (Comedor/Tienda).</summary>
    public decimal? CostoTotal { get; set; }
    public decimal? UtilidadTotal { get; set; }

    public List<LiquidacionItemDto> Items { get; set; } = [];
}

public class LiquidacionItemDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Ambiente { get; set; } = string.Empty;
    public int CantidadVendida { get; set; }
    public decimal Ingreso { get; set; }
    /// <summary>Null para Baños/Habitaciones — no tienen costo en el modelo.</summary>
    public decimal? Costo { get; set; }
    public decimal? Utilidad { get; set; }
}
