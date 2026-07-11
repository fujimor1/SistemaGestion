namespace Termales.Common.DTOs.Reporte;

public class ReporteInventarioDto
{
    public decimal ValorizacionTotal { get; set; }
    public List<ValorizacionInsumoDto> Detalle { get; set; } = [];
}

public class ValorizacionInsumoDto
{
    public string Nombre { get; set; } = string.Empty;
    public string TipoAmbiente { get; set; } = string.Empty;
    public string TipoArticulo { get; set; } = string.Empty;
    public string? Unidad { get; set; }
    public decimal StockActual { get; set; }
    public decimal PrecioReferencia { get; set; }
    public decimal Valorizacion => StockActual * PrecioReferencia;
}
