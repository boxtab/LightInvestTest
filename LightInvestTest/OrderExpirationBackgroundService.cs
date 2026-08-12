namespace LightInvestTest;

using Microsoft.AspNetCore.SignalR;

public class OrderExpirationBackgroundService
    : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

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

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Order expiration background service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope =
                    _scopeFactory.CreateScope();

                var orderService =
                    scope.ServiceProvider
                        .GetRequiredService<IOrderService>();

                var orders =
                    orderService
                        .GetOrdersForCancellation(
                            _orderLifetime)
                        .ToList();

                foreach (var order in orders)
                {
                    var cancelledOrder =
                        await orderService
                            .CancelOrderAsync(order.Id);

                    if (cancelledOrder == null)
                    {
                        continue;
                    }

                    _logger.LogInformation(
                        "Order {OrderId} cancelled automatically.",
                        cancelledOrder.Id);

                    // Очень важный момент:
                    // уведомляем только клиентов
                    // владельца этого ордера.
                    await _hubContext
                        .Clients
                        .Group(cancelledOrder.UserId)
                        .ReceiveOrderUpdate(cancelledOrder);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error in order expiration background service.");
            }

            await Task.Delay(
                _checkInterval,
                stoppingToken);
        }

        _logger.LogInformation(
            "Order expiration background service stopped.");
    }
}