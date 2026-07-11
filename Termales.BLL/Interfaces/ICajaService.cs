using Termales.Common.DTOs.Caja;

namespace Termales.BLL.Interfaces;

public interface ICajaService
{
    // Apertura
    Task<AperturaCajaDto?> ObtenerAperturaHoyAsync();
    Task<AperturaCajaDto> AbrirCajaAsync(AbrirCajaDto dto, string registradoPor);

    // Egresos
    Task<IEnumerable<EgresoCajaChicaDto>> ObtenerEgresosHoyAsync();
    Task<IEnumerable<EgresoCajaChicaDto>> ObtenerEgresosPorFechaAsync(DateTime fecha);
    Task<EgresoCajaChicaDto> RegistrarEgresoAsync(RegistrarEgresoDto dto, string registradoPor);
    Task<bool> EliminarEgresoAsync(int id);

    // Cierre
    Task<DatosCierreDto> ObtenerDatosCierreAsync();
    Task<CierreCajaDto> CerrarCajaAsync(CerrarCajaDto dto);
}
