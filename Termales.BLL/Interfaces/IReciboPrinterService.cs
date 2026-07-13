using Termales.Common.DTOs.Comprobante;

namespace Termales.BLL.Interfaces;

public interface IReciboPrinterService
{
    /// <summary>
    /// Imprime el ticket de venta en la impresora de caja (y abre el cajón de
    /// dinero). Nunca lanza: si la impresora está apagada, desconectada o
    /// deshabilitada por configuración, solo registra el error — el
    /// comprobante ya se emitió y no debe fallar por esto.
    /// </summary>
    Task ImprimirAsync(ComprobanteResultadoDto resultado, IEnumerable<ItemReciboDto> items, string clienteLabel);

    /// <summary>
    /// Imprime un ticket simple de referencia (sin abrir el cajón), usado
    /// como comprobante extra de acceso cuando una venta cubre más de un
    /// área/servicio (ej. combo de baños) y conviene un ticket aparte para
    /// controlar el ingreso a cada una. Nunca lanza, igual que ImprimirAsync.
    /// </summary>
    Task ImprimirTicketControlAsync(string titulo, string detalle);
}
