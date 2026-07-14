using System.Globalization;
using Termales.BLL.Interfaces.Sunat;
using Termales.Common.Settings;
using Termales.Entities.Models;

namespace Termales.BLL.Services.Sunat.Pdf;

public class QrContentBuilder : IQrContentBuilder
{
    public string Construir(Comprobante comprobante, EmpresaSettings empresa, string digestValueBase64)
    {
        var fechaLocal = comprobante.FechaEmision.ToUniversalTime().AddHours(-5); // Perú: UTC-5

        var campos = new[]
        {
            empresa.Ruc,
            "01", // catálogo 01: Factura
            comprobante.Serie,
            comprobante.Numero.ToString(CultureInfo.InvariantCulture),
            comprobante.Impuesto.ToString("F2", CultureInfo.InvariantCulture),
            comprobante.Total.ToString("F2", CultureInfo.InvariantCulture),
            fechaLocal.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            "6", // catálogo 06: RUC del adquirente (Factura siempre exige RUC)
            comprobante.ClienteRuc ?? "",
            digestValueBase64,
        };

        return string.Join("|", campos);
    }
}
