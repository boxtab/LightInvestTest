namespace LightInvestTest;

using System.Collections.Concurrent;

public class OrderService : IOrderService
{
    // Потокобезопасная хеш-таблица. прямой аналог Java-класса ConcurrentHashMap<UUID, Order>
    private readonly ConcurrentDictionary<Guid, Order> _orders = new();

    public Task<Order> CreateOrderAsync(OrderRequest request)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            Symbol = request.Symbol,
            Price = request.Price,
            Volume = request.Volume,
            UserId = request.UserId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _orders[order.Id] = order;

        return Task.FromResult(order);
    }

    public Task<Order?> CancelOrderAsync(Guid orderId)
    {
        if (!_orders.TryGetValue(orderId, out var order))
        {
            return Task.FromResult<Order?>(null);
        }

        if (!order.IsActive)
        {
            return Task.FromResult<Order?>(null);
        }

        order.IsActive = false;

        return Task.FromResult<Order?>(order);
    }

    public IEnumerable<Order> GetActiveOrdersForUser(string userId)
    {
        return _orders.Values
            .Where(order =>
                order.UserId == userId &&
                order.IsActive)
            .ToList();
    }

    public IEnumerable<Order> GetOrdersForCancellation(
        TimeSpan orderLifetime)
    {
        var now = DateTime.UtcNow;

        return _orders.Values
            .Where(order =>
                order.IsActive &&
                now - order.CreatedAt >= orderLifetime)
            .ToList();
    }
}