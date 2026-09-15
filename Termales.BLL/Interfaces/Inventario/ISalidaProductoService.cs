using Termales.Common.DTOs.Inventario;

namespace Termales.BLL.Interfaces.Inventario;

public interface ISalidaProductoService
{
    Task<IEnumerable<SalidaProductoDto>> ObtenerPorProductoAsync(int productoId);
    Task<SalidaProductoDto> RegistrarAsync(RegistrarSalidaProductoDto dto);
}
