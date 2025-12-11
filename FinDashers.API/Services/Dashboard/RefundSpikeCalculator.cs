using Dapper;
using Npgsql;

namespace FinDashers.API.Services.Dashboard;

public interface IRefundSpikeCalculator
{
    Task<int> CalculateScoreAsync(DateTime startDate, DateTime endDate, string? locationId);
}

public class RefundSpikeCalculator : IRefundSpikeCalculator
{
    private readonly string _connectionString;

    public RefundSpikeCalculator(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("PostgreSQL") 
            ?? throw new InvalidOperationException("PostgreSQL connection string not found");
    }

    public async Task<int> CalculateScoreAsync(DateTime startDate, DateTime endDate, string? locationId)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var whereClause = "WHERE event_code IN ('REFUND', 'CHARGEBACK')";
        var parameters = new { StartDate = startDate, EndDate = endDate, LocationId = locationId };

        if (!string.IsNullOrEmpty(locationId))
        {
            whereClause += " AND location_id = @LocationId";
        }

        // Get current period refunds
        var currentRefunds = await GetRefundCountAsync(connection, whereClause + " AND event_date >= @StartDate AND event_date <= @EndDate", parameters);

        // Get baseline (7 days before start date)
        var baselineStart = startDate.AddDays(-7);
        var baselineEnd = startDate.AddDays(-1);
        var baselineParams = new { StartDate = baselineStart, EndDate = baselineEnd, LocationId = locationId };
        var baselineRefunds = await GetRefundCountAsync(connection, whereClause + " AND event_date >= @StartDate AND event_date <= @EndDate", baselineParams);

        if (baselineRefunds == 0) return currentRefunds > 0 ? 25 : 0;

        var spikePercentage = ((double)(currentRefunds - baselineRefunds) / baselineRefunds) * 100;

        // 20% spike threshold as specified
        return spikePercentage switch
        {
            >= 100 => 25, // 100%+ spike
            >= 75 => 20,  // 75%+ spike
            >= 50 => 15,  // 50%+ spike
            >= 30 => 10,  // 30%+ spike
            >= 20 => 5,   // 20%+ spike (threshold)
            _ => 0
        };
    }

    private async Task<int> GetRefundCountAsync(NpgsqlConnection connection, string whereClause, object parameters)
    {
        var query = $"SELECT COUNT(*) FROM adyen_transactions {whereClause}";
        return await connection.QueryFirstOrDefaultAsync<int>(query, parameters);
    }
}