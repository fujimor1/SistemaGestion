namespace Termales.Entities.Models;

/// <summary>Contador de correlativo por serie, para reservar el siguiente número de forma atómica
/// (reemplaza el MAX(Numero)+1 en memoria, que tenía condición de carrera bajo concurrencia).</summary>
public class ComprobanteSerie
{
    public string Serie { get; set; } = string.Empty; // PK
    public string TipoComprobante { get; set; } = string.Empty; // NV | BI | FI | NC, informativo
    public int UltimoNumero { get; set; }
}
