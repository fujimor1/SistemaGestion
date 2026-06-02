using Microsoft.EntityFrameworkCore;
using Termales.Entities.Models;

namespace Termales.DAL.Context;

public class TermalesDbContext : DbContext
{
    public TermalesDbContext(DbContextOptions<TermalesDbContext> options) : base(options) { }

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Piscina> Piscinas => Set<Piscina>();
    public DbSet<Servicio> Servicios => Set<Servicio>();
    public DbSet<Reserva> Reservas => Set<Reserva>();
    public DbSet<ReservaServicio> ReservaServicios => Set<ReservaServicio>();
    public DbSet<Pago> Pagos => Set<Pago>();
    public DbSet<Empleado> Empleados => Set<Empleado>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TermalesDbContext).Assembly);
    }
}
