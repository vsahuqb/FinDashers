using FinDashers.API.Models.Dashboard;

namespace FinDashers.API.Services.Dashboard;

public class DashboardBroadcastService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DashboardBroadcastService> _logger;

    public DashboardBroadcastService(IServiceProvider serviceProvider, ILogger<DashboardBroadcastService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var successRateService = scope.ServiceProvider.GetRequiredService<IPaymentSuccessRateService>();
                var heatIndexService = scope.ServiceProvider.GetRequiredService<IHeatIndexCalculatorService>();
                var webSocketService = scope.ServiceProvider.GetRequiredService<IWebSocketService>();

                var endDate = DateTime.UtcNow;
                var startDate = endDate.AddDays(-1);

                var successRateTask = successRateService.CalculateSuccessRateAsync(startDate, endDate, null);
                var heatIndexTask = heatIndexService.CalculateHeatIndexAsync(startDate, endDate, null);

                await Task.WhenAll(successRateTask, heatIndexTask);

                var response = new DashboardResponse
                {
                    PaymentSuccessRate = await successRateTask,
                    PaymentHealthHeatIndex = await heatIndexTask
                };

                await webSocketService.BroadcastDashboardUpdateAsync(response);
                _logger.LogInformation("Broadcasted periodic dashboard update");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in dashboard broadcast service");
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}