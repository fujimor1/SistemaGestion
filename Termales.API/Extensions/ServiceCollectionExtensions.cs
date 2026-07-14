using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Termales.BLL.Interfaces;
using Termales.BLL.Interfaces.Comedor;
using Termales.BLL.Interfaces.Compras;
using Termales.BLL.Interfaces.Inventario;
using Termales.BLL.Interfaces.Sunat;
using Termales.BLL.Interfaces.Tienda;
using Termales.BLL.Services;
using Termales.BLL.Services.Comedor;
using Termales.BLL.Services.Compras;
using Termales.BLL.Services.Inventario;
using Termales.BLL.Services.Sunat;
using Termales.BLL.Services.Sunat.Cdr;
using Termales.BLL.Services.Sunat.Empaquetado;
using Termales.BLL.Services.Sunat.Firma;
using Termales.BLL.Services.Sunat.Pdf;
using Termales.BLL.Services.Sunat.Soap;
using Termales.BLL.Services.Sunat.Xml;
using Termales.BLL.Services.Tienda;

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
        services.AddScoped<ITurnoService, TurnoService>();
        services.AddScoped<ITipoServicioService, TipoServicioService>();
        services.AddScoped<IAforoService, AforoService>();
        services.AddScoped<IEmpleadoService, EmpleadoService>();
        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddScoped<IHabitacionService, HabitacionService>();
        services.AddScoped<IPaqueteBanioService, PaqueteBanioService>();

        // Comedor
        services.AddScoped<ICategoriaMenuService, CategoriaMenuService>();
        services.AddScoped<IItemMenuService, ItemMenuService>();
        services.AddScoped<IMesaService, MesaService>();
        services.AddScoped<IOrdenService, OrdenService>();
        services.Configure<ImpresoraComandaSettings>(config.GetSection("ImpresoraComanda"));
        services.AddScoped<IComandaPrinterService, ComandaPrinterService>();

        // Tienda
        services.AddScoped<IProductoService, ProductoService>();

        // Inventario
        services.AddScoped<IInsumoService, InsumoService>();
        services.AddScoped<IEntradaInsumoService, EntradaInsumoService>();
        services.AddScoped<IEntradaProductoService, EntradaProductoService>();
        services.AddScoped<ISalidaInsumoService, SalidaInsumoService>();

        // Dashboard
        services.AddScoped<IDashboardService, DashboardService>();

        // Reportes
        services.AddScoped<IReporteService, ReporteService>();

        // Caja
        services.AddScoped<ICajaService, CajaService>();

        // Proveedores / Compras
        services.AddScoped<IProveedorService, ProveedorService>();
        services.AddScoped<ICompraService, CompraService>();

        // Singleton: el blacklist vive toda la vida de la aplicación
        services.AddSingleton<ITokenBlacklist, TokenBlacklist>();

        // Comprobantes electrónicos (Nubefact)
        services.AddHttpContextAccessor();
        services.Configure<NubefactSettings>(config.GetSection("Nubefact"));
        services.Configure<EmpresaSettings>(config.GetSection("Empresa"));
        services.AddHttpClient("Nubefact");
        services.AddScoped<ISolicitudAnulacionService, SolicitudAnulacionService>();
        services.AddScoped<IReciboPrinterService, ReciboPrinterService>();
        services.AddScoped<IComprobanteService, ComprobanteService>();

        // Facturación electrónica directa con SUNAT (solo Factura por ahora — Boleta/NC siguen en Nubefact)
        services.Configure<SunatSettings>(config.GetSection("Sunat"));
        services.AddHttpClient<ISunatBillServiceClient, SunatBillServiceClient>(c => c.Timeout = TimeSpan.FromSeconds(60));
        services.AddScoped<IFacturaXmlBuilder, FacturaXmlBuilder>();
        services.AddScoped<IXmlDsigSigner, XmlDsigSigner>();
        services.AddScoped<IComprobanteZipBuilder, ComprobanteZipBuilder>();
        services.AddScoped<ICdrParser, CdrParser>();
        services.AddScoped<IQrContentBuilder, QrContentBuilder>();
        services.AddScoped<IRepresentacionImpresaBuilder, RepresentacionImpresaBuilder>();
        services.AddScoped<IFacturaElectronicaService, FacturaElectronicaService>();

        // Consulta de DNI/RUC (Decolecta) para autocompletar nombre/razón social
        services.Configure<ConsultaDocumentoSettings>(config.GetSection("ConsultaDocumento"));
        services.AddHttpClient("Decolecta");
        services.AddScoped<IConsultaDocumentoService, ConsultaDocumentoService>();

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

            options.Events = new JwtBearerEvents
            {
                // El cliente de SignalR (transporte WebSocket) no puede mandar el
                // header Authorization en el handshake; envía el JWT por query
                // string (?access_token=...) en su lugar. Sin esto, el puente de
                // impresión (y cualquier cliente SignalR) recibe 401 siempre.
                OnMessageReceived = ctx =>
                {
                    var accessToken = ctx.Request.Query["access_token"];
                    if (!string.IsNullOrEmpty(accessToken) && ctx.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                        ctx.Token = accessToken;
                    return Task.CompletedTask;
                },

                // Rechaza tokens cuyo JTI esté en el blacklist
                OnTokenValidated = ctx =>
                {
                    var blacklist = ctx.HttpContext.RequestServices.GetRequiredService<ITokenBlacklist>();
                    var jti = ctx.Principal?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
                    if (jti is not null && blacklist.EstaRevocado(jti))
                        ctx.Fail("Token revocado");
                    return Task.CompletedTask;
                }
            };
        });

        services.AddAuthorization();

        return services;
    }
}
