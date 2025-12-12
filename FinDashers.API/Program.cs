using FinDashers.API.Features.Webhooks.Adyen.Services;
using FinDashers.API.Services.Dashboard;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Redis
var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
var redisConnection = ConnectionMultiplexer.Connect(redisConnectionString ?? "localhost:6379");
builder.Services.AddSingleton<IConnectionMultiplexer>(redisConnection);

// Add Adyen Webhook Services
builder.Services.AddScoped<IAdyenHmacValidationService, AdyenHmacValidationService>();
builder.Services.AddScoped<IAdyenDatabaseService, AdyenDatabaseService>();
builder.Services.AddScoped<IAdyenBasicAuthorizationService, AdyenBasicAuthorizationService>();
builder.Services.AddScoped<IRedisStreamService, RedisStreamService>();

// Add Dashboard Services
builder.Services.AddScoped<IPaymentSuccessRateService, PaymentSuccessRateService>();
builder.Services.AddScoped<IHeatIndexCalculatorService, HeatIndexCalculatorService>();
builder.Services.AddScoped<IUnusualFailuresCalculator, UnusualFailuresCalculator>();
builder.Services.AddScoped<ISettlementDelayCalculator, SettlementDelayCalculator>();
builder.Services.AddScoped<IHighRiskCardCalculator, HighRiskCardCalculator>();
builder.Services.AddScoped<IRefundSpikeCalculator, RefundSpikeCalculator>();
builder.Services.AddSingleton<IWebSocketService, WebSocketService>();
builder.Services.AddHostedService<DashboardBroadcastService>();

// Add Controllers
builder.Services.AddControllers();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection(); // Disabled to prevent 307 redirects

// Use CORS
app.UseCors();

// Enable WebSocket
app.UseWebSockets();

// Map WebSocket endpoint
app.Map("/ws", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        var webSocketService = context.RequestServices.GetRequiredService<IWebSocketService>();
        await webSocketService.HandleWebSocketAsync(webSocket);
    }
    else
    {
        context.Response.StatusCode = 400;
    }
});

// Map controllers
app.MapControllers();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
