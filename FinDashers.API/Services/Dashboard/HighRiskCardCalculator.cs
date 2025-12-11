using Dapper;
using Npgsql;

namespace FinDashers.API.Services.Dashboard;

public interface IHighRiskCardCalculator
{
    Task<int> CalculateScoreAsync(DateTime startDate, DateTime endDate, string? locationId);
}

public class HighRiskCardCalculator : IHighRiskCardCalculator
{
    private readonly string _connectionString;

    public HighRiskCardCalculator(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("PostgreSQL") 
            ?? throw new InvalidOperationException("PostgreSQL connection string not found");
    }

    public async Task<int> CalculateScoreAsync(DateTime startDate, DateTime endDate, string? locationId)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var whereClause = "WHERE event_code = 'AUTHORISATION' AND event_date >= @StartDate AND event_date <= @EndDate";
        var parameters = new { StartDate = startDate, EndDate = endDate, LocationId = locationId };

        if (!string.IsNullOrEmpty(locationId))
        {
            whereClause += " AND location_id = @LocationId";
        }

        // Get total unique cards processed
        var totalCards = await GetTotalUniqueCardsAsync(connection, whereClause, parameters);
        
        if (totalCards == 0) return 0;

        // Get high-risk cards (3+ failures)
        var highRiskCards = await GetHighRiskCardsAsync(connection, whereClause, parameters);

        var riskRate = (double)highRiskCards / totalCards;

        // Map risk rate to 0-25 score
        return riskRate switch
        {
            >= 0.15 => 25, // 15%+ high-risk cards
            >= 0.12 => 20, // 12%+ high-risk cards
            >= 0.09 => 15, // 9%+ high-risk cards
            >= 0.06 => 10, // 6%+ high-risk cards
            >= 0.03 => 5,  // 3%+ high-risk cards
            _ => 0
        };
    }

    private async Task<int> GetTotalUniqueCardsAsync(NpgsqlConnection connection, string whereClause, object parameters)
    {
        // Use merchant_reference as card identifier (assuming it contains card info)
        var query = $@"
            SELECT COUNT(DISTINCT merchant_reference) 
            FROM adyen_transactions 
            {whereClause} AND merchant_reference IS NOT NULL";

        return await connection.QueryFirstOrDefaultAsync<int>(query, parameters);
    }

    private async Task<int> GetHighRiskCardsAsync(NpgsqlConnection connection, string whereClause, object parameters)
    {
        // Cards with 3+ failures
        var query = $@"
            SELECT COUNT(DISTINCT merchant_reference)
            FROM (
                SELECT merchant_reference, 
                       SUM(CASE WHEN success = false THEN 1 ELSE 0 END) as failure_count
                FROM adyen_transactions 
                {whereClause} AND merchant_reference IS NOT NULL
                GROUP BY merchant_reference
                HAVING SUM(CASE WHEN success = false THEN 1 ELSE 0 END) >= 3
            ) high_risk_cards";

        return await connection.QueryFirstOrDefaultAsync<int>(query, parameters);
    }
}