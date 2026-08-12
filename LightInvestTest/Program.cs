using LightInvestTest;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// SignalR
builder.Services.AddSignalR();

// Order service.
// Singleton нужен, потому что ордера хранятся
// в памяти приложения.
builder.Services.AddSingleton<IOrderService, OrderService>();

// Background service,
// который автоматически отменяет старые ордера.
builder.Services.AddHostedService<OrderExpirationBackgroundService>();

// OpenAPI
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.MapHub<OrdersHub>("/hub/orders");

app.Run();