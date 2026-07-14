using Termales.Common.Settings;
using Termales.Entities.Models;

namespace Termales.BLL.Interfaces.Sunat;

public interface IQrContentBuilder
{
    /// <summary>
    /// Contenido del código QR según el Anexo técnico de SUNAT (RS 340-2017 y modificatorias):
    /// RUC|TIPO_DOC|SERIE|NUMERO|IGV|TOTAL|FECHA_EMISION|TIPO_DOC_ADQUIRENTE|NUM_DOC_ADQUIRENTE|VALOR_RESUMEN,
    /// donde VALOR_RESUMEN es el ds:DigestValue en base64 (el mismo hash calculado al firmar).
    /// </summary>
    string Construir(Comprobante comprobante, EmpresaSettings empresa, string digestValueBase64);
}
