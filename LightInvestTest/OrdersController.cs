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
    public async Task<IActionResult> CreateOrder(
        [FromBody] OrderRequest request)
    {
        var validationErrors = request.Validate();
        if (validationErrors.Any())
        {
            return BadRequest(new { Errors = validationErrors });
        }

        try
        {
            // В реальном приложении здесь должна быть
            // настоящая проверка авторизации.
            var hasAccess =
                await CheckUserAccessAsync(request.UserId);

            if (!hasAccess)
            {
                return Unauthorized();
            }

            var order =
                await _orderService.CreateOrderAsync(request);

            // Уведомляем ВСЕ SignalR-соединения
            // данного пользователя.
            await _hubContext
                .Clients
                .Group(order.UserId)
                .ReceiveOrderUpdate(order);

            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error creating order for user {UserId}",
                request.UserId);

            return StatusCode(
                500,
                "Internal server error.");
        }
    }

    private Task<bool> CheckUserAccessAsync(string userId)
    {
        // Временная заглушка для тестового задания.
        return Task.FromResult(
            !string.IsNullOrWhiteSpace(userId));
    }
}