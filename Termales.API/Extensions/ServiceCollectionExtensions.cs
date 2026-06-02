using Microsoft.EntityFrameworkCore;
using Termales.BLL.Interfaces;
using Termales.BLL.Services;
using Termales.DAL.Context;
using Termales.DAL.UnitOfWork;

namespace Termales.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTermalesServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<TermalesDbContext>(opt =>
            opt.UseNpgsql(config.GetConnectionString("TermalesDb")));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IClienteService, ClienteService>();
        services.AddScoped<IPiscinaService, PiscinaService>();
        services.AddScoped<IReservaService, ReservaService>();
        services.AddScoped<IServicioService, ServicioService>();
        services.AddScoped<IPagoService, PagoService>();

        return services;
    }
}
