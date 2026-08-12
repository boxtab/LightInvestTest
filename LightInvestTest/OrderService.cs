using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;

public class Order
{
    public Guid Id { get; set; }
    public string Symbol { get; set; }
    public decimal Price { get; set; }
    public int Volume { get; set; }
    public string UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}

public record OrderRequest(string Symbol, decimal Price, int Volume, string UserId);

public interface IOrderService
{
    Task<Order> CreateOrderAsync(OrderRequest request);
    Task<bool> CancelOrderAsync(Guid orderId);
    IEnumerable<Order> GetActiveOrdersForUser(string userId);
}

public class OrderService : IOrderService
{
    // Потокобезопасный аналог ConcurrentHashMap для хранения ордеров в памяти
    private readonly ConcurrentDictionary<Guid, Order> _orders = new();
    private readonly IMemoryCache _cache;

    public OrderService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task<Order> CreateOrderAsync(OrderRequest request)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            Symbol = request.Symbol,
            Price = request.Price,
            Volume = request.Volume,
            UserId = request.UserId,
            CreatedAt = DateTime.UtcNow, // Используем UTC, как требует правильная архитектура
            IsActive = true
        };

        _orders[order.Id] = order;
        return Task.FromResult(order);
    }

    public Task<bool> CancelOrderAsync(Guid orderId)
    {
        if (_orders.TryGetValue(orderId, out var order) && order.IsActive)
        {
            order.IsActive = false;
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public IEnumerable<Order> GetActiveOrdersForUser(string userId)
    {
        return _orders.Values
            .Where(o => o.UserId == userId && o.IsActive)
            .ToList();
    }
}