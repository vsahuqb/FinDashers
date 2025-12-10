using FinDashers.API.Features.Webhooks.Adyen.Services;
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

// Add Controllers
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

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
