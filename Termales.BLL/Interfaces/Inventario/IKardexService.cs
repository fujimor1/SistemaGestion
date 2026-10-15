using Termales.Common.DTOs.Inventario;

namespace Termales.BLL.Interfaces.Inventario;

public interface IKardexService
{
    Task<KardexInsumoDto?> ObtenerKardexInsumoAsync(int insumoId, DateTime desde, DateTime hasta);
    Task<KardexInsumoDto?> ObtenerKardexProductoAsync(int productoId, DateTime desde, DateTime hasta);
    Task<IEnumerable<KardexResumenDto>> ObtenerResumenGeneralAsync(DateTime desde, DateTime hasta, string? tipoAmbiente);
}
