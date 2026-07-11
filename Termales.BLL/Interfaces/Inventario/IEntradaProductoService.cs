using Termales.Common.DTOs.Inventario;

namespace Termales.BLL.Interfaces.Inventario;

public interface IEntradaProductoService
{
    Task<IEnumerable<EntradaProductoDto>> ObtenerPorProductoAsync(int productoId);
    Task<EntradaProductoDto> RegistrarAsync(RegistrarEntradaProductoDto dto);
}
