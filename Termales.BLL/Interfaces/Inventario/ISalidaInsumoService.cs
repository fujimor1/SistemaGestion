using Termales.Common.DTOs.Inventario;

namespace Termales.BLL.Interfaces.Inventario;

public interface ISalidaInsumoService
{
    Task<IEnumerable<SalidaInsumoDto>> ObtenerPorInsumoAsync(int insumoId);
    Task<IEnumerable<SalidaInsumoDto>> ObtenerPorFechaAsync(DateTime fecha);
    Task<SalidaInsumoDto> RegistrarAsync(RegistrarSalidaInsumoDto dto);
}
