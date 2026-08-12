namespace LightInvestTest;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService; // Модификатор джава final

    private readonly IHubContext<OrdersHub, IOrderClient>
        _hubContext;

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
        var validationErrors = request.Validate();
        if (validationErrors.Any())
        {
            return BadRequest(new { Errors = validationErrors });
        }

        try
        {
            if (!await AuthorizeUserAsync(request.UserId))
            {
                return Unauthorized();
            }

            var order = await ProcessOrderCreationAsync(request);

            return Ok(order);
        }
        catch (Exception ex)
        {
            LogOrderCreationError(ex, request?.UserId);
            return StatusCode(500, "Internal server error.");
        }
    }

    // 1. Отдельный метод на проверку доступа
    private async Task<bool> AuthorizeUserAsync(string userId)
    {
        // В реальном приложении здесь будет настоящая проверка
        return await Task.FromResult(!string.IsNullOrWhiteSpace(userId));
    }

    // 2. Отдельный метод на создание ордера и нотификацию по WebSocket
    private async Task<Order> ProcessOrderCreationAsync(OrderRequest request)
    {
        var order = await _orderService.CreateOrderAsync(request);

        await _hubContext.Clients
            .Group(order.UserId)
            .ReceiveOrderUpdate(order);

        return order;
    }

    // 3. Отдельный метод на логирование ошибок
    private void LogOrderCreationError(Exception ex, string? userId)
    {
        _logger.LogError(
            ex,
            "Error creating order for user {UserId}",
            userId);
    }
}
