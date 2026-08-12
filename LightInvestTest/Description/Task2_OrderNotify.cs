namespace LightInvestTest.Description;


/**
 * Задание №2 - описание и схема работы.
 */
public class Task2_OrderNotify
{
    
    /*
     *
                    СЕРВЕР
              ASP.NET Core / C#
                       │
        ┌──────────────┼──────────────┐
        │              │              │
   REST API       OrderService    BackgroundService
   /api/orders        │              │
        │             │              │
        │             ↓              │
        │       ConcurrentDictionary │
        │        (кэш ордеров)       │
        │                            │
        └──────────────┬─────────────┘
                       │
                  OrdersHub
                  /hub/orders
                       │
          ┌────────────┼────────────┐
          │            │            │
       Client A     Client B     Client C
       UserId=1     UserId=2     UserId=3
          │            │            │
        React        React        React
     * 
     */
    
}