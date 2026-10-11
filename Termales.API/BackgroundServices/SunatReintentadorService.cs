using Termales.BLL.Interfaces;
using Termales.DAL.UnitOfWork;

namespace Termales.API.BackgroundServices;

public class SunatReintentadorService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SunatReintentadorService> _logger;
    private static readonly TimeSpan Intervalo = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan PausaEntreEnvios = TimeSpan.FromSeconds(3);

    public SunatReintentadorService(IServiceScopeFactory scopeFactory, ILogger<SunatReintentadorService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _logger.LogInformation("[SUNAT-Reintento] Servicio iniciado. Intervalo: {Min} min", Intervalo.TotalMinutes);

        // Espera inicial para que el servidor termine de arrancar antes del primer ciclo.
        await Task.Delay(TimeSpan.FromSeconds(30), ct);

        while (!ct.IsCancellationRequested)
        {
            try
            {
                await ProcesarPendientesAsync(ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "[SUNAT-Reintento] Error inesperado en el ciclo");
            }

            await Task.Delay(Intervalo, ct);
        }
    }

    private async Task ProcesarPendientesAsync(CancellationToken ct)
    {
        // Scope propio para obtener la lista — evita contaminar el DbContext de los envíos.
        List<int> ids;
        using (var scope = _scopeFactory.CreateScope())
        {
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var pendientes = await uow.ComprobantesSunat.ObtenerPendientesAsync();
            ids = pendientes.Select(p => p.ComprobanteId).ToList();
        }

        if (ids.Count == 0) return;

        _logger.LogInformation("[SUNAT-Reintento] {Count} documento(s) pendiente(s)", ids.Count);

        foreach (var id in ids)
        {
            if (ct.IsCancellationRequested) break;

            // Scope separado por documento para que el DbContext parta limpio en cada envío.
            using var scope = _scopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IComprobanteService>();

            try
            {
                var resultado = await service.ReenviarSunatAsync(id);

                if (resultado.Exito && resultado.Data?.Aceptado == true)
                    _logger.LogInformation("[SUNAT-Reintento] id={Id}: aceptado por SUNAT", id);
                else
                    _logger.LogWarning("[SUNAT-Reintento] id={Id}: {Msg}", id, resultado.Mensaje);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "[SUNAT-Reintento] id={Id}: excepción no controlada", id);
            }

            await Task.Delay(PausaEntreEnvios, ct);
        }
    }
}
