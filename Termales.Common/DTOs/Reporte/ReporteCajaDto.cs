namespace Termales.Common.DTOs.Reporte;

public class ReporteCajaDto
{
    public string  Mes                      { get; set; } = string.Empty;
    public int     DiasConApertura          { get; set; }
    public int     DiasConCierre            { get; set; }
    public decimal TotalVentasSistema       { get; set; }
    public decimal TotalEgresosCajaChica    { get; set; }
    public decimal TotalEfectivoContado     { get; set; }
    public decimal TotalYapeContado         { get; set; }
    public decimal TotalTransferenciaContado{ get; set; }
    public decimal TotalContado             { get; set; }
    public decimal DiferenciaTotal          { get; set; }
    public List<ResumenDiarioCajaDto> PorDia { get; set; } = [];
}

public class ResumenDiarioCajaDto
{
    public DateOnly Fecha               { get; set; }
    public bool     TieneApertura       { get; set; }
    public decimal  MontoApertura       { get; set; }
    public decimal  VentasSistema       { get; set; }
    public decimal  EgresosCajaChica    { get; set; }
    public bool     TieneCierre         { get; set; }
    public decimal  EfectivoContado     { get; set; }
    public decimal  YapeContado         { get; set; }
    public decimal  TransferenciaContado{ get; set; }
    public decimal  TotalContado        { get; set; }
    public decimal  Diferencia          { get; set; }
    public string   Estado              { get; set; } = string.Empty;
}
