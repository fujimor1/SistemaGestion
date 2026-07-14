namespace Termales.Common.DTOs.Sunat;

public class ResultadoEmisionSunatDto
{
    public bool Aceptado { get; set; }
    public int? CdrCodigo { get; set; }
    public string? CdrDescripcion { get; set; }
    public byte[]? RepresentacionImpresaPdf { get; set; }
}
