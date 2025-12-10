using FinDashers.Worker.Services;

namespace FinDashers.Worker.Workers;

public class WebhookProcessorWorker : BackgroundService
{
    private readonly IRedisStreamConsumerService _consumerService;
    private readonly ILogger<WebhookProcessorWorker> _logger;

    public WebhookProcessorWorker(
        IRedisStreamConsumerService consumerService,
        ILogger<WebhookProcessorWorker> logger)
    {
        _consumerService = consumerService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("=== Webhook Processor Worker starting ===");
        
        try
        {
            _logger.LogInformation("Initializing Redis Stream Consumer Service...");
            await _consumerService.StartConsumingAsync(stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("=== Webhook Processor Worker is stopping (cancellation requested) ===");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"=== Webhook Processor Worker encountered an error: {ex.Message} ===");
            throw;
        }
        finally
        {
            _logger.LogInformation("=== Webhook Processor Worker has stopped ===");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Webhook Processor Worker is stopping");
        await base.StopAsync(cancellationToken);
    }
}
