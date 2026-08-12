var builder = WebApplication.CreateBuilder(args);

// Добавляем контроллеры (аналог @RestController в Spring)
builder.Services.AddControllers();

// Добавляем кэш в памяти (IMemoryCache)
builder.Services.AddMemoryCache();

// Регистрируем наш сервис как Singleton (чтобы кэш ордеров жил в памяти приложения)
builder.Services.AddSingleton<IOrderService, OrderService>();

// Добавляем SignalR для работы с WebSockets
builder.Services.AddSignalR();

// Открываем OpenAPI/Swagger для удобной проверки
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Подключаем маппинг контроллеров
app.MapControllers();

// Подключаем маршрут для SignalR Hub
app.MapHub<OrdersHub>("/hub/orders");

app.Run();