using Termales.Common.DTOs.Inventario;

namespace Termales.BLL.Interfaces.Inventario;

public interface IInsumoService
{
    Task<IEnumerable<InsumoDto>> ObtenerPorAmbienteAsync(string tipoAmbiente);
    Task<InsumoDto?> ObtenerPorIdAsync(int id);
    Task<InsumoDto> CrearAsync(CrearInsumoDto dto);
    Task<InsumoDto?> ActualizarAsync(int id, ActualizarInsumoDto dto);
    Task<bool> EliminarAsync(int id);
}
