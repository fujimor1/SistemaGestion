using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Termales.BLL.Services.Comedor;

[Authorize]
public class ComandaHub : Hub
{
    public async Task UnirseACocina() =>
        await Groups.AddToGroupAsync(Context.ConnectionId, "cocina");

    public async Task UnirseACajero() =>
        await Groups.AddToGroupAsync(Context.ConnectionId, "cajero");

    public async Task UnirseComoMesero(string usuarioId) =>
        await Groups.AddToGroupAsync(Context.ConnectionId, $"mesero-{usuarioId}");

    public async Task SolicitarCuenta(int ordenId) =>
        await Clients.Group("cajero").SendAsync("CuentaSolicitada", ordenId);

    /// <summary>
    /// Se une el "puente de impresión" — un cliente local (en la PC del
    /// negocio, junto a la impresora) que recibe las comandas por este canal
    /// y las imprime, ya que la API (en la nube) no tiene acceso directo a
    /// la impresora física.
    /// </summary>
    public async Task UnirseComoImpresora() =>
        await Groups.AddToGroupAsync(Context.ConnectionId, "impresoras");
}
