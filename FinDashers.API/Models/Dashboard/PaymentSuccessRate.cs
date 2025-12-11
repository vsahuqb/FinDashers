namespace FinDashers.API.Models.Dashboard;

public class PaymentSuccessRate
{
    public decimal DailySuccessRate { get; set; }
    public decimal WeeklySuccessRate { get; set; }
    public List<PaymentMethodRate> PaymentMethodRates { get; set; } = new();
    public List<LocationRate> LocationRates { get; set; } = new();
    public List<TerminalRate> TerminalRates { get; set; } = new();
}

public class PaymentMethodRate
{
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal SuccessRate { get; set; }
    public int TotalTransactions { get; set; }
}

public class LocationRate
{
    public string LocationId { get; set; } = string.Empty;
    public decimal SuccessRate { get; set; }
    public int TotalTransactions { get; set; }
}

public class TerminalRate
{
    public string TerminalId { get; set; } = string.Empty;
    public decimal SuccessRate { get; set; }
    public int TotalTransactions { get; set; }
}