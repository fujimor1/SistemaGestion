namespace Termales.Common.DTOs.Inventario;

public class KardexMovimientoDto
{
    public DateTime Fecha { get; set; }
    public string Tipo { get; set; } = string.Empty;   // "Entrada" | "Salida"
    public string Concepto { get; set; } = string.Empty; // "Compra", "Consumo diario", "Merma", etc.
    public string? Referencia { get; set; }              // Nro de compra u otro
    public decimal? CostoUnitario { get; set; }
    public decimal Entrada { get; set; }
    public decimal Salida { get; set; }
    public decimal Saldo { get; set; }
    public decimal? ValorEntrada { get; set; }
    public decimal? ValorSalida { get; set; }
    public decimal ValorSaldo { get; set; }
    public string? Responsable { get; set; }
    public string? Observacion { get; set; }
}

public class KardexInsumoDto
{
    public int InsumoId { get; set; }
    /// <summary>"insumo" | "producto"</summary>
    public string TipoArticulo { get; set; } = "insumo";
    public string Nombre { get; set; } = string.Empty;
    public string? Unidad { get; set; }
    public string TipoAmbiente { get; set; } = string.Empty;
    public decimal StockActual { get; set; }
    public decimal SaldoInicial { get; set; }
    public decimal TotalEntradas { get; set; }
    public decimal TotalSalidas { get; set; }
    public decimal ValorSaldoInicial { get; set; }
    public decimal ValorTotalEntradas { get; set; }
    public decimal ValorTotalSalidas { get; set; }
    public decimal ValorSaldoFinal { get; set; }
    public IEnumerable<KardexMovimientoDto> Movimientos { get; set; } = [];
}

public class KardexResumenDto
{
    public int InsumoId { get; set; }
    /// <summary>"insumo" | "producto"</summary>
    public string TipoArticulo { get; set; } = "insumo";
    public string Nombre { get; set; } = string.Empty;
    public string? Unidad { get; set; }
    public string TipoAmbiente { get; set; } = string.Empty;
    public decimal SaldoInicial { get; set; }
    public decimal TotalEntradas { get; set; }
    public decimal TotalSalidas { get; set; }
    public decimal SaldoFinal { get; set; }
    public decimal StockMinimo { get; set; }
    public decimal ValorSaldoFinal { get; set; }
    public decimal CostoUnitario { get; set; }
}
