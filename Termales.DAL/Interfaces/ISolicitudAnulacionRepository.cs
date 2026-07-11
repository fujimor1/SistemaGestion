using Termales.Entities.Models;

namespace Termales.DAL.Interfaces;

public interface ISolicitudAnulacionRepository : IGenericRepository<SolicitudAnulacion>
{
    Task<IEnumerable<SolicitudAnulacion>> ObtenerPendientesAsync();
    Task<IEnumerable<SolicitudAnulacion>> ObtenerHistorialAsync(DateOnly desde, DateOnly hasta);
    Task<SolicitudAnulacion?> ObtenerPorComprobanteAsync(int comprobanteId);
}
