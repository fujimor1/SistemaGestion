using Microsoft.EntityFrameworkCore;
using Termales.Entities.Models;
using Termales.Entities.Models.Caja;
using Termales.Entities.Models.Comedor;
using Termales.Entities.Models.Compras;
using Termales.Entities.Models.Seguridad;
using Termales.Entities.Models.Inventario;
using Termales.Entities.Models.Tienda;

namespace Termales.DAL.Context;

public class TermalesDbContext : DbContext
{
    public TermalesDbContext(DbContextOptions<TermalesDbContext> options) : base(options) { }

    // Módulo principal
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Piscina> Piscinas => Set<Piscina>();
    public DbSet<Habitacion> Habitaciones => Set<Habitacion>();
    public DbSet<Servicio> Servicios => Set<Servicio>();
    public DbSet<Reserva> Reservas => Set<Reserva>();
    public DbSet<ReservaServicio> ReservaServicios => Set<ReservaServicio>();
    public DbSet<Pago> Pagos => Set<Pago>();
    public DbSet<Empleado> Empleados => Set<Empleado>();

    // Módulo de seguridad
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<UsuarioRol> UsuarioRoles => Set<UsuarioRol>();

    // Módulo de baños termales
    public DbSet<TipoServicio> TiposServicio => Set<TipoServicio>();
    public DbSet<Turno> Turnos => Set<Turno>();
    public DbSet<Aforo> Aforos => Set<Aforo>();
    public DbSet<PaqueteBanio> PaquetesBanio => Set<PaqueteBanio>();
    public DbSet<PaqueteBanioTipoServicio> PaqueteBanioTiposServicio => Set<PaqueteBanioTipoServicio>();

    // Módulo de comedor
    public DbSet<CategoriaMenu> CategoriasMenu => Set<CategoriaMenu>();
    public DbSet<ItemMenu> ItemsMenu => Set<ItemMenu>();
    public DbSet<Mesa> Mesas => Set<Mesa>();
    public DbSet<Orden> Ordenes => Set<Orden>();
    public DbSet<OrdenDetalle> OrdenDetalles => Set<OrdenDetalle>();

    // Módulo de tienda
    public DbSet<Producto> Productos => Set<Producto>();

    // Módulo de inventario
    public DbSet<Insumo> Insumos => Set<Insumo>();
    public DbSet<EntradaInsumo> EntradasInsumo => Set<EntradaInsumo>();
    public DbSet<EntradaProducto> EntradasProducto => Set<EntradaProducto>();
    public DbSet<SalidaInsumo> SalidasInsumo => Set<SalidaInsumo>();

    // Comprobantes electrónicos
    public DbSet<Comprobante> Comprobantes => Set<Comprobante>();
    public DbSet<ComprobanteDetalle> ComprobanteDetalles => Set<ComprobanteDetalle>();
    public DbSet<SolicitudAnulacion> SolicitudesAnulacion => Set<SolicitudAnulacion>();

    // Módulo de caja
    public DbSet<AperturaCaja> AperturasCaja => Set<AperturaCaja>();
    public DbSet<EgresoCajaChica> EgresosCajaChica => Set<EgresoCajaChica>();
    public DbSet<CierreCaja> CierresCaja => Set<CierreCaja>();

    // Módulo de compras
    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
    public DbSet<Compra> Compras => Set<Compra>();
    public DbSet<DetalleCompra> DetalleCompras => Set<DetalleCompra>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("public");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TermalesDbContext).Assembly);
    }
}
