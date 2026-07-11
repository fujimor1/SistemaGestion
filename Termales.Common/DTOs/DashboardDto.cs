namespace Termales.Common.DTOs;

public class PuntoGraficoDto
{
    public string Label { get; set; } = string.Empty;
    public decimal Valor { get; set; }
}

public class DashboardComedorDto
{
    public decimal IngresoHoy { get; set; }
    public decimal IngresoSemana { get; set; }
    public decimal IngresoMes { get; set; }
    public int OrdenesHoy { get; set; }
    public int OrdenesAbiertas { get; set; }
    public List<PuntoGraficoDto> IngresosPorDia { get; set; } = new();
    public List<PuntoGraficoDto> PlatosMasVendidos { get; set; } = new();
    public List<PuntoGraficoDto> OrdenesParHora { get; set; } = new();
}

public class DashboardBaniosDto
{
    public int PersonasHoy { get; set; }
    public decimal IngresoHoy { get; set; }
    public int PersonasMes { get; set; }
    public decimal IngresoMes { get; set; }
    public List<PuntoGraficoDto> PersonasPorDia { get; set; } = new();
    public List<PuntoGraficoDto> PorHora { get; set; } = new();
    public List<PuntoGraficoDto> PorServicio { get; set; } = new();
}

public class DashboardHabitacionesDto
{
    public int ReservasHoy { get; set; }
    public int ReservasMes { get; set; }
    public decimal IngresoMes { get; set; }
    public int HabitacionesDisponibles { get; set; }
    public int HabitacionesTotal { get; set; }
    public List<PuntoGraficoDto> ReservasPorDia { get; set; } = new();
    public List<PuntoGraficoDto> PorDiaSemana { get; set; } = new();
    public List<PuntoGraficoDto> IngresosPorDia { get; set; } = new();
}

public class StockBajoDto
{
    public string Nombre { get; set; } = string.Empty;
    public int Stock { get; set; }
    public decimal Precio { get; set; }
}

public class DashboardTiendaDto
{
    public decimal IngresoHoy { get; set; }
    public decimal IngresoSemana { get; set; }
    public decimal IngresoMes { get; set; }
    public int VentasHoy { get; set; }
    public int ProductosTotales { get; set; }
    public List<PuntoGraficoDto> IngresosPorDia { get; set; } = new();
    public List<PuntoGraficoDto> VentasPorHora { get; set; } = new();
    public List<StockBajoDto> ProductosStockBajo { get; set; } = new();
}
