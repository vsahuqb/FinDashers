using Dapper;
using Npgsql;

namespace FinDashers.API.Services.Dashboard;

public interface ISettlementDelayCalculator
{
    Task<int> CalculateScoreAsync(DateTime startDate, DateTime endDate, string? locationId);
}

public class SettlementDelayCalculator : ISettlementDelayCalculator
{
    private readonly string _connectionString;

    public SettlementDelayCalculator(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("PostgreSQL") 
            ?? throw new InvalidOperationException("PostgreSQL connection string not found");
    }

    public async Task<int> CalculateScoreAsync(DateTime startDate, DateTime endDate, string? locationId)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var whereClause = "WHERE event_code = 'AUTHORISATION' AND success = true AND event_date >= @StartDate AND event_date <= @EndDate";
        var parameters = new { StartDate = startDate, EndDate = endDate, LocationId = locationId };

        if (!string.IsNullOrEmpty(locationId))
        {
            whereClause += " AND location_id = @LocationId";
        }

        // Get total successful transactions
        var totalTransactions = await GetTotalTransactionsAsync(connection, whereClause, parameters);
        
        if (totalTransactions == 0) return 0;

        // Get delayed settlements (T+1 = next business day)
        var delayedTransactions = await GetDelayedTransactionsAsync(connection, whereClause, parameters);

        var delayRate = (double)delayedTransactions / totalTransactions;

        // Map delay rate to 0-25 score
        return delayRate switch
        {
            >= 0.20 => 25, // 20%+ delayed
            >= 0.15 => 20, // 15%+ delayed
            >= 0.10 => 15, // 10%+ delayed
            >= 0.05 => 10, // 5%+ delayed
            >= 0.02 => 5,  // 2%+ delayed
            _ => 0
        };
    }

    private async Task<int> GetTotalTransactionsAsync(NpgsqlConnection connection, string whereClause, object parameters)
    {
        var query = $"SELECT COUNT(*) FROM adyen_transactions {whereClause}";
        return await connection.QueryFirstOrDefaultAsync<int>(query, parameters);
    }

    private async Task<int> GetDelayedTransactionsAsync(NpgsqlConnection connection, string whereClause, object parameters)
    {
        // For now, simulate delayed settlements as transactions older than 1 day without corresponding settlement event
        var query = $@"
            SELECT COUNT(*) 
            FROM adyen_transactions 
            {whereClause} 
            AND event_date < @DelayThreshold
            AND NOT EXISTS (
                SELECT 1 FROM adyen_transactions s 
                WHERE s.psp_reference = adyen_transactions.psp_reference 
                AND s.event_code IN ('CAPTURE', 'SETTLEMENT')
            )";

        var delayThreshold = DateTime.UtcNow.AddDays(-1);
        var extendedParams = new DynamicParameters(parameters);
        extendedParams.Add("DelayThreshold", delayThreshold);

        return await connection.QueryFirstOrDefaultAsync<int>(query, extendedParams);
    }
}