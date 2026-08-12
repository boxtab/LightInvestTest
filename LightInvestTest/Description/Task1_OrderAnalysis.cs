namespace LightInvestTest.Description;

/**
 * Задание №1 — анализ и обоснование исправлений.
 *
 * Данный файл содержит разбор ошибок и проблем,
 * выявленных в исходном коде задания,
 * а также пояснение предложенных вариантов исправления.
 */

public class Task1_OrderAnalysis
{
    /*
     *
     [HttpPost("orders")]
// Java / Spring Boot аналог:
// @PostMapping("/orders")
public async Task<IActionResult> CreateOrder([FromBody] OrderRequest request)
{
    try
    {
        // Проверка доступа
        var hasAccess = CheckUserAccessAsync(request.UserId);

        // ❌ ОШИБКА №1: здесь пропущен await.
        //
        // CheckUserAccessAsync(), скорее всего, возвращает Task<bool>,
        // а не bool.
        //
        // Сейчас:
        // var hasAccess = CheckUserAccessAsync(request.UserId);
        //
        // hasAccess имеет тип:
        // Task<bool>
        //
        // Нужно:
        // var hasAccess = await CheckUserAccessAsync(request.UserId);
        //
        // После await:
        // Task<bool> → bool
        //
        // Java-аналог:
        // CompletableFuture<Boolean> future = checkUserAccessAsync(userId);
        // Boolean hasAccess = future.join();
        //
        // Но важно: C# await не блокирует поток так, как join()/get().

        if (!hasAccess)
        {
            return Unauthorized();

            // Java / Spring Boot аналог:
            // return ResponseEntity.status(401).build();
        }

        // Создание ордера
        var order = new Order
        {
            Id = Guid.NewGuid(),
            // Java-аналог:
            // UUID.randomUUID()

            Symbol = request.Symbol,
            Price = request.Price,
            Volume = request.Volume,
            UserId = request.UserId,

            CreatedAt = DateTime.Now

            // ❌ ПРОБЛЕМА №2: используется локальное время сервера.
            //
            // Лучше:
            // CreatedAt = DateTime.UtcNow
            //
            // Это позволяет хранить единое время в UTC
            // независимо от часового пояса сервера.
            //
            // Java-аналог:
            // Instant.now()
            //
            // Для backend-систем обычно лучше хранить
            // временные метки в UTC.
        };

        // Сохранение в БД
        await SaveOrderAsync(order);

        // Здесь await уже присутствует — это правильно.
        //
        // Если SaveOrderAsync() возвращает Task,
        // await означает:
        //
        // "Дождись завершения сохранения,
        // но не блокируй поток во время ожидания".
        //
        // Java-аналог по смыслу:
        // CompletableFuture / async database operation.

        // Отправка уведомления
        await NotifyOrderCreated(order);

        // Здесь await тоже присутствует — правильно.
        //
        // Сначала дождались сохранения ордера,
        // затем отправляем уведомление.
        //
        // То есть:
        //
        // SaveOrderAsync()
        //       ↓
        //     await
        //       ↓
        // сохранение завершено
        //       ↓
        // NotifyOrderCreated()
        //
        // Уведомление получает уже созданный order.

        return Ok(order);
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error: " + ex.Message);

        // ❌ ПРОБЛЕМА №3: Console.WriteLine — плохой вариант
        // для нормального production API.
        //
        // Лучше использовать ILogger.
        //
        // Java-аналог:
        // private static final Logger log =
        //     LoggerFactory.getLogger(...);
        //
        // И затем:
        // log.error("Error creating order", ex);

        return BadRequest("Something went wrong");

        // ❌ ПРОБЛЕМА №4:
        //
        // catch (Exception) ловит вообще любое исключение,
        // включая ошибки сервера, БД и программные ошибки.
        //
        // При этом возвращается:
        //
        // 400 Bad Request
        //
        // Но 400 означает:
        // "клиент прислал неправильный запрос".
        //
        // Если, например, упала БД, клиент ни в чём не виноват.
        //
        // Тогда правильнее вернуть:
        //
        // 500 Internal Server Error
        //
        // Например:
        // return StatusCode(500, "Internal server error");
        //
        // В production ещё лучше использовать
        // централизованную обработку исключений
        // через middleware / exception handler,
        // вместо try/catch в каждом controller.
    }
}
     *
     * 
     */
}