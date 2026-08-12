namespace LightInvestTest;

using Microsoft.AspNetCore.SignalR;

// Аналог: @Component + фоновый поток / @Scheduled в Spring
public class OrderExpirationBackgroundService
    : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    // // доступ к WebSocket (SignalR) вне Hub-а
    private readonly IHubContext<OrdersHub, IOrderClient>
        _hubContext;

    private readonly ILogger<OrderExpirationBackgroundService>
        _logger;

    private readonly TimeSpan _orderLifetime =
        TimeSpan.FromSeconds(30);

    private readonly TimeSpan _checkInterval =
        TimeSpan.FromSeconds(1);

    public OrderExpirationBackgroundService(
        IServiceScopeFactory scopeFactory,
        IHubContext<OrdersHub, IOrderClient> hubContext,
        ILogger<OrderExpirationBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _hubContext = hubContext;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Order expiration background service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Всю начинку вынесли в отдельный приватный метод
                await ProcessExpiredOrdersAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in order expiration background service.");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("Order expiration background service stopped.");
    }

    // Вынесли логику обработки в отдельный метод
    private async Task ProcessExpiredOrdersAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();

        var orders = orderService.GetOrdersForCancellation(_orderLifetime).ToList();

        foreach (var order in orders)
        {
            await CancelAndNotifyOrderAsync(orderService, order.Id);
        }
    }

    // Отдельно отмена + отправка WebSocket уведомления
    private async Task CancelAndNotifyOrderAsync(IOrderService orderService, Guid orderId)
    {
        var cancelledOrder = await orderService.CancelOrderAsync(orderId);
        if (cancelledOrder == null) return;

        _logger.LogInformation("Order {OrderId} cancelled automatically.", cancelledOrder.Id);

        await _hubContext.Clients
            .Group(cancelledOrder.UserId)
            .ReceiveOrderUpdate(cancelledOrder);
    }
}