using FinDashers.API.Features.Webhooks.Adyen.Models;
using StackExchange.Redis;
using System.Text.Json;

namespace FinDashers.Worker.Services;

public interface IRedisStreamConsumerService
{
    Task StartConsumingAsync(CancellationToken cancellationToken);
}

public class RedisStreamConsumerService : IRedisStreamConsumerService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IWebhookDatabaseService _databaseService;
    private readonly ILogger<RedisStreamConsumerService> _logger;
    private readonly IConfiguration _configuration;

    private string StreamKey => _configuration["RedisStreamConsumer:StreamKey"] ?? "webhook-events";
    private string ConsumerGroup => _configuration["RedisStreamConsumer:ConsumerGroup"] ?? "webhook-processors";
    private string ConsumerId => _configuration["RedisStreamConsumer:ConsumerId"] ?? "worker-1";
    private int BatchSize => int.Parse(_configuration["RedisStreamConsumer:BatchSize"] ?? "10");
    private int PollIntervalMs => int.Parse(_configuration["RedisStreamConsumer:PollIntervalMs"] ?? "1000");

    public RedisStreamConsumerService(
        IConnectionMultiplexer redis,
        IWebhookDatabaseService databaseService,
        ILogger<RedisStreamConsumerService> logger,
        IConfiguration configuration)
    {
        _redis = redis;
        _databaseService = databaseService;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task StartConsumingAsync(CancellationToken cancellationToken)
    {
        var db = _redis.GetDatabase();
        _logger.LogInformation($"Starting Redis Stream consumer for stream '{StreamKey}' with consumer group '{ConsumerGroup}'");

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                // Ensure consumer group exists before each polling cycle
                await EnsureConsumerGroupAsync(db);

                // Read pending messages first
                var pendingMessages = await ReadPendingMessagesAsync(db);
                if (pendingMessages.Count > 0)
                {
                    _logger.LogInformation($"Processing {pendingMessages.Count} pending messages");
                    await ProcessMessagesAsync(db, pendingMessages);
                }

                // Read new messages
                var messages = await ReadNewMessagesAsync(db);
                if (messages.Count > 0)
                {
                    _logger.LogInformation($"Processing {messages.Count} new messages");
                    await ProcessMessagesAsync(db, messages);
                }
                else
                {
                    _logger.LogDebug($"No new messages, waiting {PollIntervalMs}ms before next poll");
                    await Task.Delay(PollIntervalMs, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Consumer cancellation requested");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in consumer loop: {ex.Message}");
                await Task.Delay(PollIntervalMs, cancellationToken);
            }
        }
    }

    private async Task EnsureConsumerGroupAsync(IDatabase db)
    {
        try
        {
            // Check if stream exists first
            var streamExists = await db.KeyExistsAsync(StreamKey);
            if (!streamExists)
            {
                _logger.LogDebug($"Stream '{StreamKey}' doesn't exist yet");
                return;
            }

            // Try to create consumer group
            await db.StreamCreateConsumerGroupAsync(StreamKey, ConsumerGroup, StreamPosition.Beginning);
            _logger.LogInformation($"Created consumer group '{ConsumerGroup}' for stream '{StreamKey}'");
        }
        catch (RedisCommandException ex) when (ex.Message.Contains("BUSYGROUP"))
        {
            // Consumer group already exists - this is fine
            _logger.LogDebug($"Consumer group '{ConsumerGroup}' already exists");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"Error ensuring consumer group: {ex.Message}");
        }
    }

    private async Task<List<StreamEntry>> ReadPendingMessagesAsync(IDatabase db)
    {
        try
        {
            var pendingInfo = await db.StreamPendingAsync(StreamKey, ConsumerGroup);
            if (pendingInfo.PendingMessageCount == 0)
                return new List<StreamEntry>();

            var messages = await db.StreamReadGroupAsync(
                StreamKey,
                ConsumerGroup,
                ConsumerId,
                "0",
                count: BatchSize);

            return messages?.ToList() ?? new List<StreamEntry>();
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Error reading pending messages: {ex.Message}");
            return new List<StreamEntry>();
        }
    }

    private async Task<List<StreamEntry>> ReadNewMessagesAsync(IDatabase db)
    {
        try
        {
            var messages = await db.StreamReadGroupAsync(
                StreamKey,
                ConsumerGroup,
                ConsumerId,
                ">",
                count: BatchSize);

            return messages?.ToList() ?? new List<StreamEntry>();
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Error reading new messages: {ex.Message}");
            return new List<StreamEntry>();
        }
    }

    private async Task ProcessMessagesAsync(IDatabase db, List<StreamEntry> messages)
    {
        foreach (var message in messages)
        {
            try
            {
                var dataEntry = message.Values.FirstOrDefault(x => x.Name == "data");
                if (dataEntry.Value.IsNullOrEmpty)
                {
                    _logger.LogWarning($"Message {message.Id} has no data field");
                    await AckMessageAsync(db, message.Id);
                    continue;
                }

                // Deserialize the transaction
                var jsonData = dataEntry.Value.ToString();
                var transaction = JsonSerializer.Deserialize<AdyenTransaction>(jsonData);

                if (transaction == null)
                {
                    _logger.LogWarning($"Failed to deserialize message {message.Id}");
                    await AckMessageAsync(db, message.Id);
                    continue;
                }

                // Save to database
                await _databaseService.InsertAdyenTransactionAsync(transaction);

                // Acknowledge the message
                await AckMessageAsync(db, message.Id);

                _logger.LogInformation($"Successfully processed webhook event {message.Id} for PSP Reference: {transaction.PspReference}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing message {message.Id}: {ex.Message}");
                // Don't acknowledge on error - message will be retried
            }
        }
    }

    private async Task AckMessageAsync(IDatabase db, RedisValue messageId)
    {
        try
        {
            await db.StreamAcknowledgeAsync(StreamKey, ConsumerGroup, messageId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Error acknowledging message {messageId}: {ex.Message}");
        }
    }
}
