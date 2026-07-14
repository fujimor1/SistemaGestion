using Termales.Common.Settings;
using Termales.Entities.Models;

namespace Termales.BLL.Interfaces.Sunat;

public interface IRepresentacionImpresaBuilder
{
    /// <summary>Genera el PDF de la representación impresa de la Factura, con el código QR en la parte inferior.</summary>
    byte[] Generar(Comprobante comprobante, EmpresaSettings empresa, string digestValueBase64);
}
