using Microsoft.AspNetCore.SignalR;

public interface IOrderClient
{
    Task ReceiveOrderUpdate(Order order);
    Task ReceiveInitialOrders(List<Order> orders);
}

public class OrdersHub : Hub<IOrderClient>
{
    private readonly IOrderService _orderService;

    public OrdersHub(IOrderService orderService)
    {
        _orderService = orderService;
    }

    public override async Task OnConnectedAsync()
    {
        // Извлекаем UserId из параметров подключения (например: /hub/orders?userId=user1)
        var httpContext = Context.GetHttpContext();
        var userId = httpContext?.Request.Query["userId"].ToString();

        if (!string.IsNullOrEmpty(userId))
        {
            // Добавляем соединение клиента в группу его пользователя
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);

            // Отправляем текущие активные ордера при подключении
            var activeOrders = _orderService.GetActiveOrdersForUser(userId).ToList();
            await Clients.Caller.ReceiveInitialOrders(activeOrders);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var httpContext = Context.GetHttpContext();
        var userId = httpContext?.Request.Query["userId"].ToString();

        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
        }

        await base.OnDisconnectedAsync(exception);
    }
}
