using Termales.DAL.Context;
using Termales.DAL.Interfaces;
using Termales.DAL.Interfaces.Comedor;
using Termales.DAL.Interfaces.Compras;
using Termales.DAL.Interfaces.Tienda;
using Termales.DAL.Repositories;
using Termales.DAL.Repositories.Comedor;
using Termales.DAL.Repositories.Compras;
using Termales.DAL.Interfaces.Inventario;
using Termales.DAL.Repositories.Inventario;
using Termales.DAL.Repositories.Tienda;

namespace Termales.DAL.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly TermalesDbContext _context;
    private bool _disposed;

    public IClienteRepository Clientes { get; }
    public IReservaRepository Reservas { get; }
    public IPiscinaRepository Piscinas { get; }
    public IHabitacionRepository Habitaciones { get; }
    public IHabitacionItemRepository HabitacionItems { get; }
    public IServicioRepository Servicios { get; }
    public IPagoRepository Pagos { get; }
    public IEmpleadoRepository Empleados { get; }
    public ITipoServicioRepository TiposServicio { get; }
    public ITurnoRepository Turnos { get; }
    public IAforoRepository Aforos { get; }
    public ICategoriaMenuRepository CategoriasMenu { get; }
    public IItemMenuRepository ItemsMenu { get; }
    public IMesaRepository Mesas { get; }
    public IOrdenRepository Ordenes { get; }
    public IComprobanteRepository Comprobantes { get; }
    public ISolicitudAnulacionRepository SolicitudesAnulacion { get; }
    public IComprobanteSunatRepository ComprobantesSunat { get; }
    public IComprobanteSerieRepository ComprobanteSeries { get; }
    public IProductoRepository Productos { get; }
    public IInsumoRepository Insumos { get; }
    public IEntradaInsumoRepository EntradasInsumo { get; }
    public IEntradaProductoRepository EntradasProducto { get; }
    public ISalidaInsumoRepository SalidasInsumo { get; }
    public IProveedorRepository Proveedores { get; }
    public ICompraRepository Compras { get; }
    public IPaqueteBanioRepository PaquetesBanio { get; }

    public UnitOfWork(TermalesDbContext context)
    {
        _context = context;
        Clientes = new ClienteRepository(context);
        Reservas = new ReservaRepository(context);
        Piscinas = new PiscinaRepository(context);
        Habitaciones = new HabitacionRepository(context);
        HabitacionItems = new HabitacionItemRepository(context);
        Servicios = new ServicioRepository(context);
        Pagos = new PagoRepository(context);
        Empleados = new EmpleadoRepository(context);
        TiposServicio = new TipoServicioRepository(context);
        Turnos = new TurnoRepository(context);
        Aforos = new AforoRepository(context);
        CategoriasMenu = new CategoriaMenuRepository(context);
        ItemsMenu = new ItemMenuRepository(context);
        Mesas = new MesaRepository(context);
        Ordenes = new OrdenRepository(context);
        Comprobantes = new ComprobanteRepository(context);
        SolicitudesAnulacion = new SolicitudAnulacionRepository(context);
        ComprobantesSunat = new ComprobanteSunatRepository(context);
        ComprobanteSeries = new ComprobanteSerieRepository(context);
        Productos = new ProductoRepository(context);
        Insumos = new InsumoRepository(context);
        EntradasInsumo = new EntradaInsumoRepository(context);
        EntradasProducto = new EntradaProductoRepository(context);
        SalidasInsumo = new SalidaInsumoRepository(context);
        Proveedores = new ProveedorRepository(context);
        Compras = new CompraRepository(context);
        PaquetesBanio = new PaqueteBanioRepository(context);
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
