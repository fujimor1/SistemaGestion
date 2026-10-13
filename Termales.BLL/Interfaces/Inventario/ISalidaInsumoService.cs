using Termales.Common.DTOs.Inventario;

namespace Termales.BLL.Interfaces.Inventario;

public interface ISalidaInsumoService
{
    Task<IEnumerable<SalidaInsumoDto>> ObtenerPorInsumoAsync(int insumoId);
    Task<IEnumerable<SalidaInsumoDto>> ObtenerPorFechaAsync(DateTime fecha);
    Task<IEnumerable<SalidaInsumoDto>> ObtenerHistorialAsync(DateTime desde, DateTime hasta, int? insumoId, string? motivo);
    Task<SalidaInsumoDto> RegistrarAsync(RegistrarSalidaInsumoDto dto, string? registradoPor = null);
    Task<IEnumerable<SalidaInsumoDto>> RegistrarLoteAsync(RegistrarSalidaLoteDto dto, string registradoPor);
}
