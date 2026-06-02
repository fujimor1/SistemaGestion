using Microsoft.EntityFrameworkCore;
using Termales.Entities.Models;
using Termales.Entities.Models.Seguridad;

namespace Termales.DAL.Context;

public class TermalesDbContext : DbContext
{
    public TermalesDbContext(DbContextOptions<TermalesDbContext> options) : base(options) { }

    // Módulo principal
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Piscina> Piscinas => Set<Piscina>();
    public DbSet<Servicio> Servicios => Set<Servicio>();
    public DbSet<Reserva> Reservas => Set<Reserva>();
    public DbSet<ReservaServicio> ReservaServicios => Set<ReservaServicio>();
    public DbSet<Pago> Pagos => Set<Pago>();
    public DbSet<Empleado> Empleados => Set<Empleado>();

    // Módulo de seguridad
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<UsuarioRol> UsuarioRoles => Set<UsuarioRol>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("public");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TermalesDbContext).Assembly);
    }
}
