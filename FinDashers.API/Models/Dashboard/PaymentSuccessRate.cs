namespace FinDashers.API.Models.Dashboard;

public class PaymentSuccessRate
{
    // Aggregated Success Rates
    public decimal DailySuccessRate { get; set; }
    public decimal WeeklySuccessRate { get; set; }
    
    // Financial Metrics
    public decimal NetSales { get; set; }
    public decimal AvgTicket { get; set; }
    public int ApprovedCount { get; set; }
    public int DeclinedCount { get; set; }
    public int TotalTransactions { get; set; }
    
    // Hourly Trends
    public List<HourlyTrend> HourlyTrends { get; set; } = new();
    
    // Funnel Metrics
    public FunnelMetrics FunnelMetrics { get; set; } = new();
    
    // Breakdowns
    public List<PaymentMethodRate> PaymentMethodRates { get; set; } = new();
    public List<LocationRate> LocationRates { get; set; } = new();
    public List<TerminalRate> TerminalRates { get; set; } = new();
    
    // Payment Status Counts
    public List<PaymentStatusCount> StatusCounts { get; set; } = new();
    
    // Health Components
    public List<HealthComponent> Components { get; set; } = new();
}

public class PaymentMethodRate
{
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal SuccessRate { get; set; }
    public int TotalTransactions { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
}

public class LocationRate
{
    public string LocationId { get; set; } = string.Empty;
    public decimal SuccessRate { get; set; }
    public int TotalTransactions { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
}

public class TerminalRate
{
    public string TerminalId { get; set; } = string.Empty;
    public decimal SuccessRate { get; set; }
    public int TotalTransactions { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
}

public class HourlyTrend
{
    public int Hour { get; set; }
    public decimal SuccessRate { get; set; }
    public int TotalTransactions { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
}

public class FunnelMetrics
{
    public int Initiated { get; set; }
    public int Authorized { get; set; }
    public int Captured { get; set; }
    public int SubmittedForSettlement { get; set; }
    public int CancelledOrRefunded { get; set; }
}

public class PaymentStatusCount
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class HealthComponent
{
    public string Name { get; set; } = string.Empty;
    public int Score { get; set; }
    public int MaxScore { get; set; } = 25;
}