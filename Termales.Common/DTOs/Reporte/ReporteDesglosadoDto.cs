namespace Termales.Common.DTOs.Reporte;

public class IngresoDiarioDesglosadoDto
{
    public DateOnly Fecha            { get; set; }
    public decimal  Restaurant       { get; set; }
    public decimal  BaniosTermales   { get; set; }
    public decimal  Tienda           { get; set; }
    public decimal  Hospedaje        { get; set; }
    public decimal  TotalRegistrado  { get; set; }
    public bool     TieneApertura    { get; set; }
    public decimal  SaldoInicial     { get; set; }
    public bool     TieneCierre      { get; set; }
    public decimal  Efectivo         { get; set; }
    public decimal  Yape             { get; set; }
    public decimal  NetoYape         { get; set; }
    public decimal  Egreso           { get; set; }
    public decimal? Faltante         { get; set; }
    public decimal? Sobrante         { get; set; }
    public decimal  NetoEfectivo     { get; set; }
    public decimal  NetoTotal        { get; set; }
}

public class EgresoAdministrativoDesgloseDto
{
    public DateOnly Fecha       { get; set; }
    public string   Concepto    { get; set; } = string.Empty;
    public decimal  Monto       { get; set; }
    public string   Responsable { get; set; } = string.Empty;
}

/// <summary>Una línea de Compra registrada como FACTURA, con el rubro (Tienda/Restaurante/
/// Hospedaje/Pozas) inferido de sus líneas de detalle — solo facturas formales; el resto de
/// gastos (sin factura, sin proveedor claro) se sigue llevando manualmente.</summary>
public class GastoProveedorDesgloseDto
{
    public string   Rubro           { get; set; } = string.Empty;
    public string   Proveedor       { get; set; } = string.Empty;
    public DateOnly Fecha           { get; set; }
    public string   TipoComprobante { get; set; } = string.Empty;
    public string?  Serie           { get; set; }
    public int?     Numero          { get; set; }
    public decimal  Total           { get; set; }
}

public class ReporteDesglosadoDto
{
    public string Desde { get; set; } = string.Empty;
    public string Hasta { get; set; } = string.Empty;

    public List<IngresoDiarioDesglosadoDto> Ingresos { get; set; } = [];

    public List<EgresoAdministrativoDesgloseDto> EgresosAdministrativos { get; set; } = [];
    public decimal TotalEgresosAdministrativos { get; set; }

    public List<GastoProveedorDesgloseDto> ComprasFacturadas { get; set; } = [];
    public decimal TotalComprasFacturadas { get; set; }

    public decimal TotalIngresos { get; set; }
    public decimal TotalEgresos  { get; set; }
    public decimal Utilidad      { get; set; }
}
