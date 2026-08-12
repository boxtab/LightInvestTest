using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IHubContext<OrdersHub, IOrderClient> _hubContext;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        IOrderService orderService, 
        IHubContext<OrdersHub, IOrderClient> hubContext,
        ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _hubContext = hubContext;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] OrderRequest request)
    {
        if (request == null)
        {
            return BadRequest("Request cannot be null.");
        }

        try
        {
            // Исправление из Задания 1: добавлен ключевой await для проверки доступа
            var hasAccess = await CheckUserAccessAsync(request.UserId);
            if (!hasAccess)
            {
                return Unauthorized();
            }

            // Создаем ордер через сервис
            var order = await _orderService.CreateOrderAsync(request);

            // Рассылаем обновление всем клиентам этого пользователя через SignalR группу
            await _hubContext.Clients.Group(request.UserId).ReceiveOrderUpdate(order);

            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating order for user {UserId}", request.UserId);
            return StatusCode(500, "Internal server error");
        }
    }

    // Заглушка метода проверки доступа (упомянутая в Задании 1)
    private Task<bool> CheckUserAccessAsync(string userId)
    {
        // В реальном приложении здесь идет проверка прав
        return Task.FromResult(!string.IsNullOrEmpty(userId));
    }
}
