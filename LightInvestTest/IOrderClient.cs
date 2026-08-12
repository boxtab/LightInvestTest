namespace LightInvestTest;

public interface IOrderClient
{
    Task ReceiveOrderUpdate(Order order);

    Task ReceiveInitialOrders(List<Order> orders);
}
