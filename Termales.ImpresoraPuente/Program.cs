using System.Net.Sockets;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Termales.ImpresoraPuente;

var baseDir = AppContext.BaseDirectory;
Logger.Ruta = Path.Combine(baseDir, "puente.log");
AppDomain.CurrentDomain.UnhandledException += (_, e) =>
    Logger.Log($"ERROR FATAL no controlado: {e.ExceptionObject}");

var cfg = JsonSerializer.Deserialize<Config>(
    await File.ReadAllTextAsync(Path.Combine(baseDir, "appsettings.json")),
    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
    ?? throw new InvalidOperationException("No se pudo leer appsettings.json");

Logger.Log("=== Puente de Impresión Collpa iniciado ===");
Logger.Log($"API: {cfg.ApiBaseUrl}");
Logger.Log($"Modo de impresión: {cfg.Modo}" + (string.Equals(cfg.Modo, "usb", StringComparison.OrdinalIgnoreCase) ? $" (impresora: {cfg.NombreImpresora})" : $" ({cfg.Ip}:{cfg.Puerto})"));
Logger.Log($"Cuenta de Windows: {Environment.UserDomainName}\\{Environment.UserName}");

string token;
try
{
    token = await LoginAsync(cfg);
    Logger.Log("Login OK.");
}
catch (Exception ex)
{
    Logger.Log($"ERROR FATAL: no se pudo iniciar sesión contra la API ({ex.Message}). El puente se cierra.");
    return;
}

var connection = new HubConnectionBuilder()
    .WithUrl($"{cfg.ApiBaseUrl.TrimEnd('/')}/hubs/comanda", options =>
    {
        options.AccessTokenProvider = () => Task.FromResult<string?>(token);
    })
    .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(30) })
    .ConfigureLogging(logging =>
    {
        logging.AddConsole();
        logging.SetMinimumLevel(LogLevel.Warning);
    })
    .Build();

connection.On<string>("ImprimirComanda", async ticketBase64 =>
{
    try
    {
        var bytes = Convert.FromBase64String(ticketBase64);
        await ImprimirAsync(cfg, bytes);
        Logger.Log("Comanda impresa OK.");
    }
    catch (Exception ex)
    {
        Logger.Log($"ERROR al imprimir comanda: {ex.Message}");
    }
});

connection.On<string>("ImprimirBoleta", async ticketBase64 =>
{
    try
    {
        var bytes = Convert.FromBase64String(ticketBase64);
        await ImprimirAsync(cfg, bytes);
        Logger.Log("Boleta impresa OK.");
    }
    catch (Exception ex)
    {
        Logger.Log($"ERROR al imprimir boleta: {ex.Message}");
    }
});

connection.Reconnecting += ex =>
{
    Logger.Log("Conexión perdida, reintentando...");
    return Task.CompletedTask;
};

connection.Reconnected += async _ =>
{
    Logger.Log("Reconectado, uniéndose a los grupos de impresión...");
    await UnirseSegunRolAsync(connection, cfg);
};

connection.Closed += async ex =>
{
    Logger.Log($"Conexión cerrada: {ex?.Message}. Reintentando en 5s...");
    await Task.Delay(5000);
    await ConectarConReintentosAsync(connection, cfg);
};

await ConectarConReintentosAsync(connection, cfg);
Logger.Log($"Conectado (rol: {cfg.Rol}) y esperando comandas/boletas. Presiona Ctrl+C para salir.");

var salir = new TaskCompletionSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; salir.SetResult(); };
await salir.Task;

await connection.DisposeAsync();
return;

static async Task<string> LoginAsync(Config cfg)
{
    using var http = new HttpClient { BaseAddress = new Uri(cfg.ApiBaseUrl) };
    var respuesta = await http.PostAsJsonAsync("api/auth/login", new { email = cfg.Email, password = cfg.Password });
    respuesta.EnsureSuccessStatusCode();
    var body = await respuesta.Content.ReadFromJsonAsync<AuthResponse>();
    var token = body?.Data?.Token;
    return string.IsNullOrEmpty(token) ? throw new InvalidOperationException("La respuesta de login no trajo token") : token;
}

static async Task ConectarConReintentosAsync(HubConnection connection, Config cfg)
{
    while (true)
    {
        try
        {
            await connection.StartAsync();
            await UnirseSegunRolAsync(connection, cfg);
            return;
        }
        catch (Exception ex)
        {
            Logger.Log($"No se pudo conectar al hub: {ex.Message}. Reintentando en 5s...");
            await Task.Delay(5000);
        }
    }
}

// "cocina" | "caja" | "ambas" — con "ambas" se une a los dos grupos, así una
// sola impresora conectada a esta PC puede imprimir comandas y boletas
// mientras el negocio no tenga una impresora dedicada para cada una.
static async Task UnirseSegunRolAsync(HubConnection connection, Config cfg)
{
    var rol = cfg.Rol.Trim().ToLowerInvariant();
    if (rol is "cocina" or "ambas")
        await connection.InvokeAsync("UnirseComoImpresoraCocina");
    if (rol is "caja" or "ambas")
        await connection.InvokeAsync("UnirseComoImpresoraCaja");
}

static async Task ImprimirAsync(Config cfg, byte[] bytes)
{
    if (string.Equals(cfg.Modo, "usb", StringComparison.OrdinalIgnoreCase))
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("El modo \"usb\" solo funciona en Windows");
        await ImprimirUsbAsync(cfg, bytes);
    }
    else
    {
        using var cliente = new TcpClient();
        var conexion = cliente.ConnectAsync(cfg.Ip, cfg.Puerto);
        if (await Task.WhenAny(conexion, Task.Delay(cfg.TimeoutMs)) != conexion)
            throw new TimeoutException($"No se pudo conectar a la impresora {cfg.Ip}:{cfg.Puerto}");

        using var stream = cliente.GetStream();
        await stream.WriteAsync(bytes);
        await stream.FlushAsync();
    }
}

[System.Runtime.Versioning.SupportedOSPlatform("windows")]
static Task ImprimirUsbAsync(Config cfg, byte[] bytes)
{
#pragma warning disable CA1416 // Ya se valida OperatingSystem.IsWindows() en el llamador (ImprimirAsync)
    return Task.Run(() => RawPrinterHelper.SendBytesToPrinter(cfg.NombreImpresora, bytes));
#pragma warning restore CA1416
}

// La API envuelve las respuestas en ApiResponse<T> ({ exito, mensaje, data, errores });
// el token real vive en response.data.token, no en la raíz.
file class AuthResponse
{
    [JsonPropertyName("data")]
    public AuthData? Data { get; set; }
}

file class AuthData
{
    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;
}

file static class Logger
{
    public static string Ruta = string.Empty;
    private static readonly object candado = new();

    public static void Log(string mensaje)
    {
        var linea = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {mensaje}";
        Console.WriteLine(linea);
        if (string.IsNullOrEmpty(Ruta)) return;
        try
        {
            lock (candado) File.AppendAllText(Ruta, linea + Environment.NewLine);
        }
        catch { /* si no se puede escribir el log, no debe tumbar el puente */ }
    }
}
