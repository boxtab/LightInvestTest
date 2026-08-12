namespace LightInvestTest;

public interface IOrderService
{
    Task<Order> CreateOrderAsync(OrderRequest request);

    Task<Order?> CancelOrderAsync(Guid orderId);

    IEnumerable<Order> GetActiveOrdersForUser(string userId);

    IEnumerable<Order> GetOrdersForCancellation(
        TimeSpan orderLifetime);
}
