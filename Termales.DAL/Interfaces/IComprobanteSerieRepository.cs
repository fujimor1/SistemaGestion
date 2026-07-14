namespace Termales.DAL.Interfaces;

public interface IComprobanteSerieRepository
{
    /// <summary>Reserva y devuelve el siguiente número de forma atómica para esa serie (sin condición de carrera).</summary>
    Task<int> SiguienteNumeroAsync(string serie, string tipoComprobante);
}
