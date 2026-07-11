namespace Termales.Entities.Enums;

public enum EstadoOrden
{
    Abierta    = 1,
    EnCocina   = 2,
    Lista      = 3,  // cocina terminó, regresó al mozo para entrega
    ParaCobrar = 6,  // mozo confirmó entrega, visible en caja
    Pagada     = 4,
    Cancelada  = 5
}
