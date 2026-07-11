using System.Net.Sockets;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Termales.ImpresoraPuente;

var baseDir = AppContext.BaseDirectory;
var cfg = JsonSerializer.Deserialize<Config>(
    await File.ReadAllTextAsync(Path.Combine(baseDir, "appsettings.json")),
    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
    ?? throw new InvalidOperationException("No se pudo leer appsettings.json");

Console.WriteLine("=== Puente de Impresión Collpa ===");
Console.WriteLine($"API: {cfg.ApiBaseUrl}");
Console.WriteLine($"Modo de impresión: {cfg.Modo}");

string token = await LoginAsync(cfg);
Console.WriteLine("Login OK.");

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
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Comanda impresa OK.");
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Error al imprimir: {ex.Message}");
        Console.ResetColor();
    }
});

connection.Reconnecting += ex =>
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Conexión perdida, reintentando...");
    Console.ResetColor();
    return Task.CompletedTask;
};

connection.Reconnected += async _ =>
{
    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Reconectado, uniéndose al grupo de impresión...");
    await connection.InvokeAsync("UnirseComoImpresora");
};

connection.Closed += async ex =>
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Conexión cerrada: {ex?.Message}. Reintentando en 5s...");
    Console.ResetColor();
    await Task.Delay(5000);
    await ConectarConReintentosAsync(connection);
};

await ConectarConReintentosAsync(connection);
Console.WriteLine("Conectado y esperando comandas. Presiona Ctrl+C para salir.");

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

static async Task ConectarConReintentosAsync(HubConnection connection)
{
    while (true)
    {
        try
        {
            await connection.StartAsync();
            await connection.InvokeAsync("UnirseComoImpresora");
            return;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] No se pudo conectar al hub: {ex.Message}. Reintentando en 5s...");
            Console.ResetColor();
            await Task.Delay(5000);
        }
    }
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
