using Dapper;
using Npgsql;

namespace FinDashers.API.Services.Dashboard;

public interface IUnusualFailuresCalculator
{
    Task<int> CalculateScoreAsync(DateTime startDate, DateTime endDate, string? locationId);
}

public class UnusualFailuresCalculator : IUnusualFailuresCalculator
{
    private readonly string _connectionString;

    public UnusualFailuresCalculator(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("PostgreSQL") 
            ?? throw new InvalidOperationException("PostgreSQL connection string not found");
    }

    public async Task<int> CalculateScoreAsync(DateTime startDate, DateTime endDate, string? locationId)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var whereClause = "WHERE event_code = 'AUTHORISATION'";
        var parameters = new { StartDate = startDate, EndDate = endDate, LocationId = locationId };

        if (!string.IsNullOrEmpty(locationId))
        {
            whereClause += " AND location_id = @LocationId";
        }

        // Get current period failures
        var currentFailures = await GetFailureCountAsync(connection, whereClause + " AND event_date >= @StartDate AND event_date <= @EndDate", parameters);

        // Get baseline (7 days before start date)
        var baselineStart = startDate.AddDays(-7);
        var baselineEnd = startDate.AddDays(-1);
        var baselineParams = new { StartDate = baselineStart, EndDate = baselineEnd, LocationId = locationId };
        var baselineFailures = await GetFailureCountAsync(connection, whereClause + " AND event_date >= @StartDate AND event_date <= @EndDate", baselineParams);

        if (baselineFailures == 0) return 0;

        var spikePercentage = ((double)(currentFailures - baselineFailures) / baselineFailures) * 100;

        // Map spike percentage to 0-25 score
        return spikePercentage switch
        {
            >= 100 => 25,
            >= 75 => 20,
            >= 50 => 15,
            >= 25 => 10,
            >= 10 => 5,
            _ => 0
        };
    }

    private async Task<int> GetFailureCountAsync(NpgsqlConnection connection, string whereClause, object parameters)
    {
        var query = $@"
            SELECT COUNT(*) 
            FROM adyen_transactions 
            {whereClause} AND success = false";

        return await connection.QueryFirstOrDefaultAsync<int>(query, parameters);
    }
}