using Termales.Entities.Models;

namespace Termales.DAL.Interfaces;

public interface IComprobanteSunatRepository : IGenericRepository<ComprobanteSunat>
{
    Task<ComprobanteSunat?> ObtenerPorComprobanteIdAsync(int comprobanteId);
    Task<IEnumerable<ComprobanteSunat>> ObtenerPendientesAsync();
    // Comprobantes BI/FI/NC que quedaron en "PENDIENTE DE ENVÍO A SUNAT" sin fila en comprobantes_sunat
    // (p.ej. cuando el error ocurrió antes del try/catch en FacturaElectronicaService).
    Task<IEnumerable<Comprobante>> ObtenerHuerfanosAsync();
}
