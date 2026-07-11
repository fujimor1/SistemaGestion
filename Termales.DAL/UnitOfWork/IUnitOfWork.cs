using Termales.DAL.Interfaces;
using Termales.DAL.Interfaces.Comedor;
using Termales.DAL.Interfaces.Compras;
using Termales.DAL.Interfaces.Inventario;
using Termales.DAL.Interfaces.Tienda;

namespace Termales.DAL.UnitOfWork;

public interface IUnitOfWork : IDisposable
{
    IClienteRepository Clientes { get; }
    IReservaRepository Reservas { get; }
    IPiscinaRepository Piscinas { get; }
    IHabitacionRepository Habitaciones { get; }
    IServicioRepository Servicios { get; }
    IPagoRepository Pagos { get; }
    IEmpleadoRepository Empleados { get; }
    ITipoServicioRepository TiposServicio { get; }
    ITurnoRepository Turnos { get; }
    IAforoRepository Aforos { get; }
    ICategoriaMenuRepository CategoriasMenu { get; }
    IItemMenuRepository ItemsMenu { get; }
    IMesaRepository Mesas { get; }
    IOrdenRepository Ordenes { get; }
    IComprobanteRepository Comprobantes { get; }
    ISolicitudAnulacionRepository SolicitudesAnulacion { get; }
    IProductoRepository Productos { get; }
    IInsumoRepository Insumos { get; }
    IEntradaInsumoRepository EntradasInsumo { get; }
    IEntradaProductoRepository EntradasProducto { get; }
    ISalidaInsumoRepository SalidasInsumo { get; }
    IProveedorRepository Proveedores { get; }
    ICompraRepository Compras { get; }
    IPaqueteBanioRepository PaquetesBanio { get; }

    Task<int> GuardarCambiosAsync();
}
