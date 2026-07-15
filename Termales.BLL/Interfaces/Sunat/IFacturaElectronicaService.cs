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

    /// <summary>
    /// Regenera bajo demanda la representación impresa (PDF + QR) de un comprobante ya emitido.
    /// No depende de que SUNAT haya respondido: el hash/digest se firma y persiste localmente
    /// antes de intentar el envío, así que esto funciona igual si SUNAT está caída o aún no
    /// contestó — es el respaldo real para poder entregarle el documento al cliente de todas formas.
    /// </summary>
    Task<ApiResponse<byte[]>> ObtenerRepresentacionImpresaAsync(int comprobanteId);
}
