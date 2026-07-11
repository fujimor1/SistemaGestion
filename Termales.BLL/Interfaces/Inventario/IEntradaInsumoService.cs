using Termales.Common.DTOs.Inventario;

namespace Termales.BLL.Interfaces.Inventario;

public interface IEntradaInsumoService
{
    Task<IEnumerable<EntradaInsumoDto>> ObtenerPorInsumoAsync(int insumoId);
    Task<EntradaInsumoDto> RegistrarAsync(RegistrarEntradaInsumoDto dto);
}
