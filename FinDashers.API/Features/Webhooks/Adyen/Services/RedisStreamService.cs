using StackExchange.Redis;
using System.Text.Json;

namespace FinDashers.API.Features.Webhooks.Adyen.Services;

public interface IRedisStreamService
{
    Task<string> PublishWebhookEventAsync(object eventData);
}

public class RedisStreamService : IRedisStreamService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisStreamService> _logger;
    private const string StreamKey = "webhook-events";

    public RedisStreamService(IConnectionMultiplexer redis, ILogger<RedisStreamService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task<string> PublishWebhookEventAsync(object eventData)
    {
        try
        {
            var db = _redis.GetDatabase();
            
            // Serialize the event data
            var jsonData = JsonSerializer.Serialize(eventData);
            
            // Create stream entry
            var entry = new NameValueEntry("data", jsonData);
            
            // Add to stream (Redis will auto-generate ID)
            var streamId = await db.StreamAddAsync(StreamKey, new NameValueEntry[] { entry });
            
            _logger.LogInformation($"Published webhook event to Redis Stream with ID: {streamId}");
            return streamId.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error publishing webhook event to Redis Stream: {ex.Message}");
            throw;
        }
    }
}
