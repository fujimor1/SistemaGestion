using Termales.DAL.Context;
using Termales.DAL.Interfaces;
using Termales.DAL.Repositories;

namespace Termales.DAL.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly TermalesDbContext _context;
    private bool _disposed;

    public IClienteRepository Clientes { get; }
    public IReservaRepository Reservas { get; }
    public IPiscinaRepository Piscinas { get; }
    public IServicioRepository Servicios { get; }
    public IPagoRepository Pagos { get; }
    public ITipoServicioRepository TiposServicio { get; }
    public ITurnoRepository Turnos { get; }
    public IAforoRepository Aforos { get; }

    public UnitOfWork(TermalesDbContext context)
    {
        _context = context;
        Clientes = new ClienteRepository(context);
        Reservas = new ReservaRepository(context);
        Piscinas = new PiscinaRepository(context);
        Servicios = new ServicioRepository(context);
        Pagos = new PagoRepository(context);
        TiposServicio = new TipoServicioRepository(context);
        Turnos = new TurnoRepository(context);
        Aforos = new AforoRepository(context);
    }

    public async Task<int> GuardarCambiosAsync() =>
        await _context.SaveChangesAsync();

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
            _context.Dispose();
        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
