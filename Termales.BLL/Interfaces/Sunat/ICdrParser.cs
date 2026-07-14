namespace Termales.BLL.Interfaces.Sunat;

public record ResultadoCdr(int Codigo, string Descripcion);

public interface ICdrParser
{
    /// <summary>Descomprime el ZIP del CDR y extrae el código y descripción de respuesta de SUNAT.</summary>
    ResultadoCdr Parsear(byte[] cdrZip);
}
