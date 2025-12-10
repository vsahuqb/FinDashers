using FinDashers.Worker.Services;
using FinDashers.Worker.Workers;
using StackExchange.Redis;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices((context, services) =>
{
    // Add Redis with retry configuration
    var redisConnectionString = context.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
    var configurationOptions = ConfigurationOptions.Parse(redisConnectionString);
    configurationOptions.AbortOnConnectFail = false;
    configurationOptions.ConnectTimeout = 10000;
    configurationOptions.SyncTimeout = 5000;
    configurationOptions.ConnectRetry = 3;
    configurationOptions.ReconnectRetryPolicy = new ExponentialRetry(1000);
    
    var redisConnection = ConnectionMultiplexer.Connect(configurationOptions);
    
    // Add connection event logging
    redisConnection.ConnectionFailed += (sender, e) => {
        Console.WriteLine($"Redis connection failed: {e.Exception?.Message}");
    };
    redisConnection.ConnectionRestored += (sender, e) => {
        Console.WriteLine($"Redis connection restored: {e.ConnectionType}");
    };
    redisConnection.ErrorMessage += (sender, e) => {
        Console.WriteLine($"Redis error: {e.Message}");
    };
    
    services.AddSingleton<IConnectionMultiplexer>(redisConnection);

    // Add Database Service
    services.AddScoped<IWebhookDatabaseService, WebhookDatabaseService>();

    // Add Redis Stream Consumer Service
    services.AddScoped<IRedisStreamConsumerService, RedisStreamConsumerService>();

    // Add Background Worker
    services.AddHostedService<WebhookProcessorWorker>();
});

var host = builder.Build();
await host.RunAsync();
