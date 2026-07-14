namespace Termales.BLL.Interfaces.Sunat;

public class ResultadoEnvioSunat
{
    public bool Exito { get; init; }

    /// <summary>ZIP del CDR (Constancia de Recepción), en crudo, si SUNAT aceptó/procesó el envío.</summary>
    public byte[]? CdrZip { get; init; }

    /// <summary>Presente solo si SUNAT devolvió un SOAP Fault (fallo estructural/autenticación, no de negocio).</summary>
    public string? FaultCode { get; init; }
    public string? FaultString { get; init; }

    public static ResultadoEnvioSunat Aceptado(byte[] cdrZip) => new() { Exito = true, CdrZip = cdrZip };
    public static ResultadoEnvioSunat Fallo(string faultCode, string faultString) =>
        new() { Exito = false, FaultCode = faultCode, FaultString = faultString };
}

public interface ISunatBillServiceClient
{
    Task<ResultadoEnvioSunat> EnviarAsync(string nombreArchivoZip, byte[] contenidoZip, CancellationToken ct = default);
}
