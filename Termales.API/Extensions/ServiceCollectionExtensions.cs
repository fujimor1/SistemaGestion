using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Termales.BLL.Interfaces;
using Termales.BLL.Services;
using Termales.Common.Settings;
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
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }

    public static IServiceCollection AddTermalesJwt(this IServiceCollection services, IConfiguration config)
    {
        var jwtSection = config.GetSection("JwtSettings");
        services.Configure<JwtSettings>(jwtSection);

        var jwt = jwtSection.Get<JwtSettings>()!;
        var key = Encoding.UTF8.GetBytes(jwt.SecretKey);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwt.Issuer,
                ValidAudience = jwt.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddAuthorization();

        return services;
    }
}
