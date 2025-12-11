using FinDashers.API.Models.Dashboard;

namespace FinDashers.API.Services.Dashboard;

public interface IHeatIndexCalculatorService
{
    Task<PaymentHealthHeatIndex> CalculateHeatIndexAsync(DateTime startDate, DateTime endDate, string? locationId);
}

public class HeatIndexCalculatorService : IHeatIndexCalculatorService
{
    private readonly IUnusualFailuresCalculator _unusualFailuresCalculator;
    private readonly ISettlementDelayCalculator _settlementDelayCalculator;
    private readonly IHighRiskCardCalculator _highRiskCardCalculator;
    private readonly IRefundSpikeCalculator _refundSpikeCalculator;

    public HeatIndexCalculatorService(
        IUnusualFailuresCalculator unusualFailuresCalculator,
        ISettlementDelayCalculator settlementDelayCalculator,
        IHighRiskCardCalculator highRiskCardCalculator,
        IRefundSpikeCalculator refundSpikeCalculator)
    {
        _unusualFailuresCalculator = unusualFailuresCalculator;
        _settlementDelayCalculator = settlementDelayCalculator;
        _highRiskCardCalculator = highRiskCardCalculator;
        _refundSpikeCalculator = refundSpikeCalculator;
    }

    public async Task<PaymentHealthHeatIndex> CalculateHeatIndexAsync(DateTime startDate, DateTime endDate, string? locationId)
    {
        var tasks = new[]
        {
            _unusualFailuresCalculator.CalculateScoreAsync(startDate, endDate, locationId),
            _settlementDelayCalculator.CalculateScoreAsync(startDate, endDate, locationId),
            _highRiskCardCalculator.CalculateScoreAsync(startDate, endDate, locationId),
            _refundSpikeCalculator.CalculateScoreAsync(startDate, endDate, locationId)
        };

        var scores = await Task.WhenAll(tasks);

        return new PaymentHealthHeatIndex
        {
            UnusualFailuresScore = scores[0],
            SettlementDelayScore = scores[1],
            HighRiskCardScore = scores[2],
            RefundSpikeScore = scores[3],
            TotalScore = scores.Sum()
        };
    }
}