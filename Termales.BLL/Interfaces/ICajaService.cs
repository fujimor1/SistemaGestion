using Termales.Common.DTOs.Caja;

namespace Termales.BLL.Interfaces;

public interface ICajaService
{
    // Apertura
    Task<AperturaCajaDto?> ObtenerAperturaHoyAsync();
    Task<AperturaCajaDto> AbrirCajaAsync(AbrirCajaDto dto, string registradoPor);

    /// <summary>True si hay una apertura de caja para hoy y todavía no se
    /// registró su cierre — requisito para compras, ventas y egresos.</summary>
    Task<bool> HayCajaAbiertaAsync();

    // Egresos
    Task<IEnumerable<EgresoCajaChicaDto>> ObtenerEgresosHoyAsync();
    Task<IEnumerable<EgresoCajaChicaDto>> ObtenerEgresosPorFechaAsync(DateTime fecha);
    Task<EgresoCajaChicaDto> RegistrarEgresoAsync(RegistrarEgresoDto dto, string registradoPor, int? compraId = null);
    Task<bool> EliminarEgresoAsync(int id);

    // Cierre
    Task<DatosCierreDto> ObtenerDatosCierreAsync();
    Task<CierreCajaDto> CerrarCajaAsync(CerrarCajaDto dto);
}
