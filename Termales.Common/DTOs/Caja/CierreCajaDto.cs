namespace Termales.Common.DTOs.Caja;

public class ResumenAmbienteDto
{
    public string Ambiente { get; set; } = string.Empty;
    public string NombreAmbiente { get; set; } = string.Empty;
    public int CantidadTransacciones { get; set; }
    public decimal Total { get; set; }
}

public class DatosCierreDto
{
    public decimal TotalSistema { get; set; }
    public decimal MontoApertura { get; set; }
    public decimal TotalEgresos { get; set; }
    public decimal SaldoCajaChica { get; set; }
    public List<ResumenAmbienteDto> ResumenPorAmbiente { get; set; } = new();
    public CierreCajaDto? CierreExistente { get; set; }
}

public class CierreCajaDto
{
    public int CierreCajaId { get; set; }
    public DateTime Fecha { get; set; }
    public decimal TotalSistema { get; set; }
    public decimal EfectivoFisico { get; set; }
    public decimal YapeFisico { get; set; }
    public decimal TransferenciaFisico { get; set; }
    public decimal TotalEgresos { get; set; }
    public decimal MontoApertura { get; set; }
    public decimal Diferencia { get; set; }
    public string? Observaciones { get; set; }
    public string EncargadoCierre { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; }
}

public class CerrarCajaDto
{
    public decimal EfectivoFisico { get; set; }
    public decimal YapeFisico { get; set; }
    public decimal TransferenciaFisico { get; set; }
    public string? Observaciones { get; set; }
    public string EncargadoCierre { get; set; } = string.Empty;
}
