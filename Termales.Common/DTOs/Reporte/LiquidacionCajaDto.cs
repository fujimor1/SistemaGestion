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

    // Desglose de las ventas del sistema (VentasSistema) por forma de pago real de
    // cada comprobante — Mixto se reparte entre efectivo y yape según MontoEfectivoMixto.
    // Distinto de EfectivoContado/YapeContado, que es lo que el cajero contó a mano al cerrar.
    public decimal VentasEfectivo { get; set; }
    public decimal VentasYape { get; set; }
    /// <summary>Transferencia (forma de pago legada, ya no seleccionable) u otro caso no contemplado.</summary>
    public decimal VentasOtros { get; set; }

    /// <summary>Notas de Venta del día — quedan como comprobante interno, no se envían a SUNAT.</summary>
    public decimal MontoInterno { get; set; }
    /// <summary>Boletas + Facturas del día — sí se envían/simulan ante SUNAT.</summary>
    public decimal MontoSunat { get; set; }

    /// <summary>Comprobantes del día que terminaron anulados — no cuentan en VentasSistema/IngresoTotal.</summary>
    public decimal MontoAnulado { get; set; }

    public List<VentaAmbienteDto> VentasPorAmbiente { get; set; } = [];
    public List<EgresoLiquidacionDto> EgresosDetalle { get; set; } = [];

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
    /// <summary>Comprobante al que pertenece esta línea, ej. "BI01-00123".</summary>
    public string NumeroComprobante { get; set; } = string.Empty;
    /// <summary>NV | BI | FI</summary>
    public string TipoComprobante { get; set; } = string.Empty;
}

public class VentaAmbienteDto
{
    public string Ambiente { get; set; } = string.Empty;
    public decimal Total { get; set; }
}

public class EgresoLiquidacionDto
{
    public string Concepto { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public string Responsable { get; set; } = string.Empty;
}
