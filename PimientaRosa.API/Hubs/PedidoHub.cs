using Microsoft.AspNetCore.SignalR;

namespace PimientaRosa.API.Hubs;

public class PedidoHub : Hub
{
    // El cliente se une a un grupo con su número de pedido
    // Así solo recibe notificaciones de SU pedido
    public async Task UnirseAPedido(string numeroPedido)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, numeroPedido);
    }

    public async Task SalirDePedido(string numeroPedido)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, numeroPedido);
    }
}