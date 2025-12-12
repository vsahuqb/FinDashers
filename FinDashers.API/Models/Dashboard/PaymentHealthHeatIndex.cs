namespace FinDashers.API.Models.Dashboard;

public class PaymentHealthHeatIndex
{
    public int TotalScore { get; set; }
    public int OverallScore => TotalScore;
    public int UnusualFailuresScore { get; set; }
    public int SettlementDelayScore { get; set; }
    public int HighRiskCardScore { get; set; }
    public int RefundSpikeScore { get; set; }
    public List<HealthComponent> Components { get; set; } = new();
    public string HealthStatus => TotalScore switch
    {
        >= 80 => "Critical",
        >= 60 => "Warning", 
        >= 40 => "Moderate",
        _ => "Healthy"
    };
}

public class DashboardResponse
{
    public PaymentSuccessRate PaymentSuccessRate { get; set; } = new();
    public PaymentHealthHeatIndex PaymentHealthHeatIndex { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}