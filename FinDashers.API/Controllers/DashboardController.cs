using FinDashers.API.Models.Dashboard;
using FinDashers.API.Services.Dashboard;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System.Text.Json;

namespace FinDashers.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IPaymentSuccessRateService _successRateService;
    private readonly IHeatIndexCalculatorService _heatIndexService;
    private readonly IConnectionMultiplexer _redis;
    private readonly IWebSocketService _webSocketService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        IPaymentSuccessRateService successRateService,
        IHeatIndexCalculatorService heatIndexService,
        IConnectionMultiplexer redis,
        IWebSocketService webSocketService,
        ILogger<DashboardController> logger)
    {
        _successRateService = successRateService;
        _heatIndexService = heatIndexService;
        _redis = redis;
        _webSocketService = webSocketService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<DashboardResponse>> GetDashboardData([FromQuery] DashboardRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (request.EndDate < request.StartDate)
        {
            return BadRequest("End date must be after start date");
        }

        try
        {
            var cacheKey = $"dashboard:{request.StartDate:yyyy-MM-dd}:{request.EndDate:yyyy-MM-dd}:{request.LocationId ?? "all"}";
            var db = _redis.GetDatabase();

            // Try to get from cache
            var cachedResult = await db.StringGetAsync(cacheKey);
            if (cachedResult.HasValue)
            {
                _logger.LogInformation($"Returning cached dashboard data for key: {cacheKey}");
                var cached = JsonSerializer.Deserialize<DashboardResponse>(cachedResult!);
                return Ok(cached);
            }

            // Calculate both metrics in parallel
            var successRateTask = _successRateService.CalculateSuccessRateAsync(request.StartDate, request.EndDate, request.LocationId);
            var heatIndexTask = _heatIndexService.CalculateHeatIndexAsync(request.StartDate, request.EndDate, request.LocationId);

            await Task.WhenAll(successRateTask, heatIndexTask);

            var response = new DashboardResponse
            {
                PaymentSuccessRate = await successRateTask,
                PaymentHealthHeatIndex = await heatIndexTask
            };

            // Cache for 30 seconds
            var serialized = JsonSerializer.Serialize(response);
            await db.StringSetAsync(cacheKey, serialized, TimeSpan.FromSeconds(30));

            // Broadcast to WebSocket clients
            _ = Task.Run(async () => await _webSocketService.BroadcastDashboardUpdateAsync(response));

            _logger.LogInformation($"Generated and cached dashboard data for key: {cacheKey}");
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating dashboard data");
            return StatusCode(500, "Internal server error");
        }
    }
}