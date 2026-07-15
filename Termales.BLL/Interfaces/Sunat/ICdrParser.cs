namespace Termales.BLL.Interfaces.Sunat;

/// <summary>
/// <paramref name="XmlCrudo"/>: el ApplicationResponse tal como vino, para poder auditar cualquier
/// detalle que este parser no extraiga explícitamente.
/// <paramref name="Observaciones"/>: notas de observación de SUNAT (catálogo 32/50, ej. "4237 - ..."),
/// distintas de <paramref name="Descripcion"/> — null si SUNAT no incluyó ninguna.
/// </summary>
public record ResultadoCdr(int Codigo, string Descripcion, string XmlCrudo, string? Observaciones);

public interface ICdrParser
{
    /// <summary>Descomprime el ZIP del CDR y extrae el código y descripción de respuesta de SUNAT.</summary>
    ResultadoCdr Parsear(byte[] cdrZip);
}
