using FinDashers.API.Features.Webhooks.Adyen.Services;
using FinDashers.API.Services.Dashboard;
using StackExchange.Redis;
using FinDashers.Core.Configuration;
using FinDashers.Core.Services;
using FinDashers.Core.Services.LLMProviders;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add configuration services
builder.Services.Configure<LLMProvidersConfig>(
    builder.Configuration.GetSection("LLMProviders"));
builder.Services.Configure<DatabaseConfig>(
    builder.Configuration.GetSection("ConnectionStrings"));

// Add HTTP client for LLM providers
builder.Services.AddHttpClient();

// Register LLM providers (Ollama first for local development, then Gemini)
builder.Services.AddSingleton<ILLMProvider, OllamaProvider>();
builder.Services.AddSingleton<ILLMProvider, GeminiProvider>();
builder.Services.AddSingleton<ILLMProvider, OpenAIProvider>();
builder.Services.AddSingleton<ILLMProvider, AnthropicProvider>();
builder.Services.AddSingleton<ILLMProvider, AzureOpenAIProvider>();

// Register core services
builder.Services.AddSingleton<LLMService>();
builder.Services.AddSingleton<DomainManager>(provider =>
{
    var logger = provider.GetRequiredService<ILogger<DomainManager>>();
    var contentRoot = builder.Environment.ContentRootPath;
    var domainsPath = Path.Combine(Directory.GetParent(contentRoot)!.FullName, "data", "domains");
    return new DomainManager(logger, domainsPath);
});
builder.Services.AddSingleton<EntityResolver>(provider =>
{
    var logger = provider.GetRequiredService<ILogger<EntityResolver>>();
    var contentRoot = builder.Environment.ContentRootPath;
    var entitiesPath = Path.Combine(Directory.GetParent(contentRoot)!.FullName, "data", "entities");
    return new EntityResolver(logger, entitiesPath);
});
builder.Services.AddScoped<SQLExecutor>();
builder.Services.AddScoped<NL2SQLService>();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "FinDashers NL2SQL API",
        Version = "v1",
        Description = "Natural Language to SQL API for Adyen payment data analysis"
    });
});

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

// Add Controllers
builder.Services.AddControllers();

// Configure routing to be case-insensitive
builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("AllowAll");

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
