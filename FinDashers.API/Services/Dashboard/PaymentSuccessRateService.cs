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
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var whereClause = "WHERE event_code = 'AUTHORISATION' AND event_date >= @StartDate AND event_date <= @EndDate";
        var parameters = new { StartDate = startDate, EndDate = endDate, LocationId = locationId };

        if (!string.IsNullOrEmpty(locationId))
        {
            whereClause += " AND location_id = @LocationId";
        }

        // Daily success rate
        var dailyRate = await CalculateDailyRateAsync(connection, whereClause, parameters);
        
        // Weekly success rate  
        var weeklyRate = await CalculateWeeklyRateAsync(connection, whereClause, parameters);

        // Payment method breakdown
        var paymentMethodRates = await CalculatePaymentMethodRatesAsync(connection, whereClause, parameters);

        // Location breakdown (if not filtered by location)
        var locationRates = string.IsNullOrEmpty(locationId) 
            ? await CalculateLocationRatesAsync(connection, whereClause, parameters)
            : new List<LocationRate>();

        // Terminal breakdown
        var terminalRates = await CalculateTerminalRatesAsync(connection, whereClause, parameters);

        return new PaymentSuccessRate
        {
            DailySuccessRate = dailyRate,
            WeeklySuccessRate = weeklyRate,
            PaymentMethodRates = paymentMethodRates,
            LocationRates = locationRates,
            TerminalRates = terminalRates
        };
    }

    private async Task<decimal> CalculateDailyRateAsync(NpgsqlConnection connection, string whereClause, object parameters)
    {
        var query = $@"
            SELECT 
                COALESCE(
                    ROUND(
                        (SUM(CASE WHEN success = true THEN 1 ELSE 0 END)::decimal / COUNT(*)) * 100, 2
                    ), 0
                ) as success_rate
            FROM adyen_transactions 
            {whereClause}";

        return await connection.QueryFirstOrDefaultAsync<decimal>(query, parameters);
    }

    private async Task<decimal> CalculateWeeklyRateAsync(NpgsqlConnection connection, string whereClause, object parameters)
    {
        return await CalculateDailyRateAsync(connection, whereClause, parameters);
    }

    private async Task<List<PaymentMethodRate>> CalculatePaymentMethodRatesAsync(NpgsqlConnection connection, string whereClause, object parameters)
    {
        var query = $@"
            SELECT 
                payment_method as PaymentMethod,
                COUNT(*) as TotalTransactions,
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

    private async Task<List<LocationRate>> CalculateLocationRatesAsync(NpgsqlConnection connection, string whereClause, object parameters)
    {
        var query = $@"
            SELECT 
                location_id as LocationId,
                COUNT(*) as TotalTransactions,
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

    private async Task<List<TerminalRate>> CalculateTerminalRatesAsync(NpgsqlConnection connection, string whereClause, object parameters)
    {
        var query = $@"
            SELECT 
                terminal_id as TerminalId,
                COUNT(*) as TotalTransactions,
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
}