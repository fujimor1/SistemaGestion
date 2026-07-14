using Termales.Common.DTOs.Sunat;
using Termales.Common.Wrappers;
using Termales.Entities.Models;

namespace Termales.BLL.Interfaces.Sunat;

public interface IFacturaElectronicaService
{
    /// <summary>
    /// Genera XML, firma, arma el PDF, empaqueta y envía a SUNAT; persiste el resultado en
    /// ComprobanteSunat. Idempotente por diseño (mismo comprobante, mismo serie/número) — se puede
    /// llamar de nuevo para reintentar un envío que falló por error de red (no de negocio).
    /// </summary>
    Task<ApiResponse<ResultadoEmisionSunatDto>> EmitirAsync(Comprobante comprobante);
}
