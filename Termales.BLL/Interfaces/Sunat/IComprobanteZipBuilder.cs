namespace Termales.BLL.Interfaces.Sunat;

public record ComprobanteZip(string NombreArchivo, byte[] ContenidoZip);

public interface IComprobanteZipBuilder
{
    /// <summary>Empaqueta el XML firmado en un ZIP con el nombre exacto que exige SUNAT: {RUC}-{tipoDoc}-{serie}-{numero}.zip</summary>
    ComprobanteZip Construir(string ruc, string tipoDoc, string serie, int numero, string xmlFirmado);
}
