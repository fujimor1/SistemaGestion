using Termales.DAL.Interfaces;

namespace Termales.DAL.UnitOfWork;

public interface IUnitOfWork : IDisposable
{
    IClienteRepository Clientes { get; }
    IReservaRepository Reservas { get; }
    IPiscinaRepository Piscinas { get; }
    IServicioRepository Servicios { get; }
    IPagoRepository Pagos { get; }
    ITipoServicioRepository TiposServicio { get; }
    ITurnoRepository Turnos { get; }
    IAforoRepository Aforos { get; }

    Task<int> GuardarCambiosAsync();
}
