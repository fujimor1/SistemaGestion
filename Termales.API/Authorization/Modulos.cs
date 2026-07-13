namespace Termales.API.Authorization;

/// <summary>
/// Grupos de roles por módulo, para usar en [Authorize(Roles = Modulos.X)].
/// Supervisor tiene acceso a todo; el resto de roles están acotados a un
/// subconjunto de módulos según lo definido para el negocio.
/// </summary>
public static class Modulos
{
    public const string Supervisor    = "Supervisor";
    public const string Administrador = "Administrador";
    public const string Recepcionista = "Recepcionista";
    public const string Mozo          = "Mozo";

    /// <summary>Baños Termales y Habitaciones (check-in/check-out).</summary>
    public const string BaniosHabitaciones = "Administrador,Recepcionista,Supervisor";

    /// <summary>Tienda, Caja, Inventario, y catálogo de Comedor (mesas/categorías/menú).</summary>
    public const string Operaciones = "Administrador,Supervisor";

    /// <summary>Toma y gestión de órdenes del comedor (mozo desde la app móvil, y Administrador
    /// desde caja para clientes que piden para llevar directamente en el mostrador).</summary>
    public const string ComedorOperacion = "Administrador,Mozo,Supervisor";

    /// <summary>Lectura de órdenes: además del mozo, Caja (Administrador/Supervisor) necesita
    /// verlas para cobrarlas en Facturación, aunque no las cree ni las gestione.</summary>
    public const string ComedorLectura = "Administrador,Mozo,Supervisor";

    /// <summary>Administración del sistema: usuarios y empleados.</summary>
    public const string Sistema = "Supervisor";
}
