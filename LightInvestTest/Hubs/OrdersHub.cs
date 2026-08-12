namespace LightInvestTest;

using Microsoft.AspNetCore.SignalR;

public class OrdersHub : Hub<IOrderClient>
{
    private readonly IOrderService _orderService;

    public OrdersHub(IOrderService orderService)
    {
        _orderService = orderService;
    }

    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();

        var userId = httpContext?
            .Request.Query["userId"]
            .ToString();

        if (string.IsNullOrWhiteSpace(userId))
        {
            Context.Abort();
            return;
        }

        // Все соединения одного пользователя
        // попадают в одну SignalR-группу.
        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            userId);

        // Отправляем клиенту его текущие активные ордера.
        var activeOrders =
            _orderService
                .GetActiveOrdersForUser(userId)
                .ToList();

        await Clients.Caller.ReceiveInitialOrders(activeOrders);

        await base.OnConnectedAsync();
    }
}