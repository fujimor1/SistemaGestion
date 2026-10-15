using Termales.Common.DTOs.Inventario;

namespace Termales.BLL.Interfaces.Inventario;

public interface ISalidaProductoService
{
    Task<IEnumerable<SalidaProductoDto>> ObtenerPorProductoAsync(int productoId);
    Task<SalidaProductoDto> RegistrarAsync(RegistrarSalidaProductoDto dto);
    Task<IEnumerable<SalidaProductoDto>> RegistrarLoteAsync(RegistrarSalidaProductoLoteDto dto);
    Task<IEnumerable<SalidaProductoDto>> ObtenerHistorialAsync(DateTime? desde, DateTime? hasta, int? productoId, string? motivo);
}
