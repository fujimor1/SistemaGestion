using System.Globalization;
using Termales.BLL.Interfaces.Sunat;
using Termales.Common.Settings;
using Termales.Entities.Models;

namespace Termales.BLL.Services.Sunat.Pdf;

public class QrContentBuilder : IQrContentBuilder
{
    public string Construir(Comprobante comprobante, EmpresaSettings empresa, string digestValueBase64)
    {
        var esFactura = comprobante.TipoComprobante == "FI";
        var fechaLocal = comprobante.FechaEmision.ToUniversalTime().AddHours(-5); // Perú: UTC-5

        string tipoDocAdquirente;
        string numDocAdquirente;
        if (esFactura)
        {
            tipoDocAdquirente = "6"; // catálogo 06: RUC
            numDocAdquirente = comprobante.ClienteRuc ?? "";
        }
        else if (!string.IsNullOrWhiteSpace(comprobante.ClienteDni))
        {
            tipoDocAdquirente = "1"; // catálogo 06: DNI
            numDocAdquirente = comprobante.ClienteDni;
        }
        else
        {
            tipoDocAdquirente = "1";
            numDocAdquirente = "00000000"; // mismo criterio que FacturaXmlBuilder para "cliente varios"
        }

        var campos = new[]
        {
            empresa.Ruc,
            esFactura ? "01" : "03", // catálogo 01: Factura / Boleta
            comprobante.Serie,
            comprobante.Numero.ToString(CultureInfo.InvariantCulture),
            comprobante.Impuesto.ToString("F2", CultureInfo.InvariantCulture),
            comprobante.Total.ToString("F2", CultureInfo.InvariantCulture),
            fechaLocal.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            tipoDocAdquirente,
            numDocAdquirente,
            digestValueBase64,
        };

        return string.Join("|", campos);
    }
}
