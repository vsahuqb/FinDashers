using Dapper;
using FinDashers.API.Models.Dashboard;
using Npgsql;

namespace FinDashers.API.Services.Dashboard;

public interface IPaymentSuccessRateService
{
    Task<PaymentSuccessRate> CalculateSuccessRateAsync(DateTime startDate, DateTime endDate, string? locationId);
}

public class PaymentSuccessRateService : IPaymentSuccessRateService
{
    private readonly string _connectionString;

    public PaymentSuccessRateService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("PostgreSQL") 
            ?? throw new InvalidOperationException("PostgreSQL connection string not found");
    }

    public async Task<PaymentSuccessRate> CalculateSuccessRateAsync(DateTime startDate, DateTime endDate, string? locationId)
    {
        var whereClause = "WHERE event_code = 'AUTHORISATION' AND event_date >= @StartDate AND event_date <= @EndDate";
        var parameters = new { StartDate = startDate, EndDate = endDate, LocationId = locationId };

        if (!string.IsNullOrEmpty(locationId))
        {
            whereClause += " AND location_id = @LocationId";
        }

        // Calculate all metrics in parallel with separate connections
        var financialTask = CalculateFinancialMetricsAsync(whereClause, parameters);
        var hourlyTask = CalculateHourlyTrendsAsync(whereClause, parameters);
        var funnelTask = CalculateFunnelMetricsAsync(parameters);
        var paymentMethodTask = CalculatePaymentMethodRatesAsync(whereClause, parameters);
        var locationTask = CalculateLocationRatesAsync(whereClause, parameters);
        var terminalTask = CalculateTerminalRatesAsync(whereClause, parameters);
        var statusCountsTask = CalculateStatusCountsAsync(parameters);

        await Task.WhenAll(financialTask, hourlyTask, funnelTask, paymentMethodTask, locationTask, terminalTask, statusCountsTask);

        var financialMetrics = await financialTask;
        var hourlyTrends = await hourlyTask;
        var funnelMetrics = await funnelTask;
        var paymentMethodRates = await paymentMethodTask;
        var locationRates = string.IsNullOrEmpty(locationId) ? await locationTask : new List<LocationRate>();
        var terminalRates = await terminalTask;
        var statusCounts = await statusCountsTask;

        return new PaymentSuccessRate
        {
            DailySuccessRate = financialMetrics.DailySuccessRate,
            WeeklySuccessRate = financialMetrics.WeeklySuccessRate,
            NetSales = financialMetrics.NetSales,
            AvgTicket = financialMetrics.AvgTicket,
            ApprovedCount = financialMetrics.ApprovedCount,
            DeclinedCount = financialMetrics.DeclinedCount,
            TotalTransactions = financialMetrics.TotalTransactions,
            HourlyTrends = hourlyTrends,
            FunnelMetrics = funnelMetrics,
            PaymentMethodRates = paymentMethodRates,
            LocationRates = locationRates,
            TerminalRates = terminalRates,
            StatusCounts = statusCounts,
            Components = CreateHealthComponents(financialMetrics.ApprovedCount, financialMetrics.TotalTransactions)
        };
    }

    private async Task<(decimal DailySuccessRate, decimal WeeklySuccessRate, decimal NetSales, decimal AvgTicket, int ApprovedCount, int DeclinedCount, int TotalTransactions)> CalculateFinancialMetricsAsync(string whereClause, object parameters)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = $@"
            SELECT 
                COUNT(*) as total_transactions,
                SUM(CASE WHEN success = true THEN 1 ELSE 0 END) as approved_count,
                SUM(CASE WHEN success = false THEN 1 ELSE 0 END) as declined_count,
                COALESCE(SUM(CASE WHEN success = true THEN approved_amount ELSE 0 END), 0) as net_sales,
                COALESCE(
                    ROUND(
                        (SUM(CASE WHEN success = true THEN 1 ELSE 0 END)::decimal / COUNT(*)) * 100, 2
                    ), 0
                ) as success_rate
            FROM adyen_transactions 
            {whereClause}";

        var result = await connection.QueryFirstOrDefaultAsync(query, parameters);
        
        var totalTransactions = (int)(result?.total_transactions ?? 0);
        var approvedCount = (int)(result?.approved_count ?? 0);
        var declinedCount = (int)(result?.declined_count ?? 0);
        var netSales = (decimal)(result?.net_sales ?? 0);
        var successRate = (decimal)(result?.success_rate ?? 0);
        var avgTicket = approvedCount > 0 ? Math.Round(netSales / approvedCount, 2) : 0;

        return (successRate, successRate, netSales, avgTicket, approvedCount, declinedCount, totalTransactions);
    }

    private async Task<List<HourlyTrend>> CalculateHourlyTrendsAsync(string whereClause, object parameters)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = $@"
            SELECT 
                EXTRACT(HOUR FROM event_date) as hour,
                COUNT(*) as total_transactions,
                SUM(CASE WHEN success = true THEN 1 ELSE 0 END) as success_count,
                SUM(CASE WHEN success = false THEN 1 ELSE 0 END) as failure_count,
                COALESCE(
                    ROUND(
                        (SUM(CASE WHEN success = true THEN 1 ELSE 0 END)::decimal / COUNT(*)) * 100, 2
                    ), 0
                ) as success_rate
            FROM adyen_transactions 
            {whereClause}
            GROUP BY EXTRACT(HOUR FROM event_date)
            ORDER BY hour";

        var results = await connection.QueryAsync(query, parameters);
        
        return results.Select(r => new HourlyTrend
        {
            Hour = (int)(r.hour ?? 0),
            TotalTransactions = (int)(r.total_transactions ?? 0),
            SuccessCount = (int)(r.success_count ?? 0),
            FailureCount = (int)(r.failure_count ?? 0),
            SuccessRate = (decimal)(r.success_rate ?? 0)
        }).ToList();
    }

    private async Task<FunnelMetrics> CalculateFunnelMetricsAsync(object parameters)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var whereClause = "WHERE event_date >= @StartDate AND event_date <= @EndDate";
        if (parameters.GetType().GetProperty("LocationId")?.GetValue(parameters) is string locationId && !string.IsNullOrEmpty(locationId))
        {
            whereClause += " AND location_id = @LocationId";
        }

        var query = $@"
            SELECT 
                event_code,
                COUNT(*) as count
            FROM adyen_transactions 
            {whereClause}
            GROUP BY event_code";

        var results = await connection.QueryAsync(query, parameters);
        
        var funnel = new FunnelMetrics();
        
        foreach (var result in results)
        {
            var eventCode = result.event_code?.ToString() ?? "";
            var count = (int)(result.count ?? 0);
            
            switch (eventCode.ToUpper())
            {
                case "AUTHORISATION":
                    funnel.Authorized = count;
                    break;
                case "CAPTURE":
                    funnel.Captured = count;
                    break;
                case "SUBMIT_FOR_SETTLEMENT":
                    funnel.SubmittedForSettlement = count;
                    break;
                case "CANCEL_OR_REFUND":
                case "REFUND":
                case "CANCELLATION":
                    funnel.CancelledOrRefunded += count;
                    break;
            }
        }
        
        // Initiated = total transactions
        funnel.Initiated = results.Sum(r => (int)(r.count ?? 0));
        
        return funnel;
    }

    private async Task<List<PaymentMethodRate>> CalculatePaymentMethodRatesAsync(string whereClause, object parameters)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = $@"
            SELECT 
                payment_method as PaymentMethod,
                COUNT(*) as TotalTransactions,
                SUM(CASE WHEN success = true THEN 1 ELSE 0 END) as SuccessCount,
                SUM(CASE WHEN success = false THEN 1 ELSE 0 END) as FailureCount,
                COALESCE(
                    ROUND(
                        (SUM(CASE WHEN success = true THEN 1 ELSE 0 END)::decimal / COUNT(*)) * 100, 2
                    ), 0
                ) as SuccessRate
            FROM adyen_transactions 
            {whereClause} AND payment_method IS NOT NULL
            GROUP BY payment_method
            ORDER BY TotalTransactions DESC";

        var results = await connection.QueryAsync<PaymentMethodRate>(query, parameters);
        return results.ToList();
    }

    private async Task<List<LocationRate>> CalculateLocationRatesAsync(string whereClause, object parameters)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = $@"
            SELECT 
                location_id as LocationId,
                COUNT(*) as TotalTransactions,
                SUM(CASE WHEN success = true THEN 1 ELSE 0 END) as SuccessCount,
                SUM(CASE WHEN success = false THEN 1 ELSE 0 END) as FailureCount,
                COALESCE(
                    ROUND(
                        (SUM(CASE WHEN success = true THEN 1 ELSE 0 END)::decimal / COUNT(*)) * 100, 2
                    ), 0
                ) as SuccessRate
            FROM adyen_transactions 
            {whereClause} AND location_id IS NOT NULL
            GROUP BY location_id
            ORDER BY TotalTransactions DESC";

        var results = await connection.QueryAsync<LocationRate>(query, parameters);
        return results.ToList();
    }

    private async Task<List<TerminalRate>> CalculateTerminalRatesAsync(string whereClause, object parameters)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = $@"
            SELECT 
                terminal_id as TerminalId,
                COUNT(*) as TotalTransactions,
                SUM(CASE WHEN success = true THEN 1 ELSE 0 END) as SuccessCount,
                SUM(CASE WHEN success = false THEN 1 ELSE 0 END) as FailureCount,
                COALESCE(
                    ROUND(
                        (SUM(CASE WHEN success = true THEN 1 ELSE 0 END)::decimal / COUNT(*)) * 100, 2
                    ), 0
                ) as SuccessRate
            FROM adyen_transactions 
            {whereClause} AND terminal_id IS NOT NULL
            GROUP BY terminal_id
            ORDER BY TotalTransactions DESC";

        var results = await connection.QueryAsync<TerminalRate>(query, parameters);
        return results.ToList();
    }

    private async Task<List<PaymentStatusCount>> CalculateStatusCountsAsync(object parameters)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var whereClause = "WHERE event_date >= @StartDate AND event_date <= @EndDate";
        if (parameters.GetType().GetProperty("LocationId")?.GetValue(parameters) is string locationId && !string.IsNullOrEmpty(locationId))
        {
            whereClause += " AND location_id = @LocationId";
        }

        var query = $@"
            SELECT 
                event_code as Status,
                COUNT(*) as Count
            FROM adyen_transactions 
            {whereClause}
            GROUP BY event_code
            ORDER BY Count DESC";

        var results = await connection.QueryAsync<PaymentStatusCount>(query, parameters);
        return results.ToList();
    }

    private List<HealthComponent> CreateHealthComponents(int approvedCount, int totalTransactions)
    {
        var successRate = totalTransactions > 0 ? (decimal)approvedCount / totalTransactions * 100 : 0;
        
        return new List<HealthComponent>
        {
            new() { Name = "Success Rate", Score = (int)Math.Min(successRate / 4, 25), MaxScore = 25 },
            new() { Name = "Transaction Volume", Score = Math.Min(totalTransactions / 50, 25), MaxScore = 25 },
            new() { Name = "Processing Health", Score = approvedCount > 0 ? 20 : 5, MaxScore = 25 },
            new() { Name = "System Stability", Score = 18, MaxScore = 25 }
        };
    }
}