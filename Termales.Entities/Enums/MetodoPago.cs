namespace Termales.Entities.Enums;

public enum MetodoPago
{
    Efectivo = 1,
    YapePlin = 2,
    /// <summary>Ya no seleccionable desde la UI, se conserva solo por datos históricos.</summary>
    Transferencia = 3,
    Fiado = 4,
    /// <summary>Pago dividido: una parte en Efectivo y el resto en Yape/Plin (ComprobanteMontoEfectivoMixto).</summary>
    Mixto = 5
}
