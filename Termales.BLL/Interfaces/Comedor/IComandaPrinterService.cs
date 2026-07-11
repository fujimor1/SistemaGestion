using Termales.Entities.Models.Comedor;

namespace Termales.BLL.Interfaces.Comedor;

public interface IComandaPrinterService
{
    /// <summary>
    /// Imprime el ticket de comanda para cocina. Nunca lanza: si la impresora
    /// está apagada, desconectada o deshabilitada por configuración, solo
    /// registra el error — la orden ya se creó y no debe fallar por esto.
    /// </summary>
    /// <param name="orden">Orden con Mesa, Usuario y Detalles cargados.</param>
    /// <param name="detalles">Ítems a incluir en el ticket (permite reimprimir solo lo agregado).</param>
    /// <param name="titulo">Encabezado del ticket, ej. "COMANDA NUEVA" o "AGREGADO A COMANDA".</param>
    Task ImprimirAsync(Orden orden, IEnumerable<OrdenDetalle> detalles, string titulo);
}
