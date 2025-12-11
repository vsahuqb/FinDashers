using System.Data;
using FinDashers.Core.Configuration;
using FinDashers.Core.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FinDashers.Core.Services;

public class SQLExecutor
{
    private readonly DatabaseConfig _config;
    private readonly ILogger<SQLExecutor> _logger;
    private readonly DomainManager _domainManager;

    public SQLExecutor(IOptions<DatabaseConfig> config, ILogger<SQLExecutor> logger, DomainManager domainManager)
    {
        _config = config.Value;
        _logger = logger;
        _domainManager = domainManager;
    }

    public async Task<QueryResult> ExecuteScriptAsync(List<string> sqlScript, string domain)
    {
        var result = new QueryResult
        {
            SqlScript = sqlScript,
            Success = false
        };

        try
        {
            var domainContext = _domainManager.GetDomainContext(domain);
            if (domainContext == null)
            {
                result.Error = $"Domain '{domain}' not found";
                return result;
            }

            var connectionString = GetConnectionString(domain, domainContext.DatabasePath);
            
            _logger.LogDebug("Executing SQL script with {StatementCount} statements for domain '{Domain}'", 
                sqlScript.Count, domain);

            using var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync();

            var lastResult = new List<dynamic>();
            var columnNames = new List<string>();
            var rowCount = 0;

            foreach (var statement in sqlScript)
            {
                if (string.IsNullOrWhiteSpace(statement)) continue;

                _logger.LogDebug("Executing SQL: {SQL}", statement);

                using var command = new SqliteCommand(statement, connection);
                
                if (IsSelectStatement(statement))
                {
                    // Execute SELECT and capture results
                    using var reader = await command.ExecuteReaderAsync();
                    
                    // Get column names
                    columnNames.Clear();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        columnNames.Add(reader.GetName(i));
                    }

                    // Read data
                    lastResult.Clear();
                    while (await reader.ReadAsync())
                    {
                        var row = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                            row[reader.GetName(i)] = value;
                        }
                        lastResult.Add(row);
                    }
                    
                    rowCount = lastResult.Count;
                }
                else
                {
                    // Execute non-SELECT statement
                    var affectedRows = await command.ExecuteNonQueryAsync();
                    _logger.LogDebug("Statement affected {AffectedRows} rows", affectedRows);
                }
            }

            result.Success = true;
            result.Data = lastResult;
            result.ColumnNames = columnNames;
            result.RowCount = rowCount;

            _logger.LogInformation("Successfully executed SQL script for domain '{Domain}', returned {RowCount} rows", 
                domain, rowCount);

            return result;
        }
        catch (Exception ex)
        {
            result.Error = ex.Message;
            _logger.LogError(ex, "Error executing SQL script for domain '{Domain}': {Error}", domain, ex.Message);
            return result;
        }
    }

    private string GetConnectionString(string domain, string databasePath)
    {
        // Try domain-specific connection string first
        var connectionString = domain.ToLowerInvariant() switch
        {
            "payments" => _config.Payments,
            "adyen" => _config.Adyen,
            _ => null
        };

        // Fallback to database path from domain context
        if (string.IsNullOrEmpty(connectionString))
        {
            connectionString = $"Data Source={databasePath}";
        }

        return connectionString;
    }

    private static bool IsSelectStatement(string sql)
    {
        var trimmedSql = sql.Trim();
        return trimmedSql.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase) ||
               trimmedSql.StartsWith("WITH", StringComparison.OrdinalIgnoreCase); // Common Table Expressions
    }

    public async Task<bool> TestConnectionAsync(string domain)
    {
        try
        {
            var domainContext = _domainManager.GetDomainContext(domain);
            if (domainContext == null) return false;

            var connectionString = GetConnectionString(domain, domainContext.DatabasePath);
            
            using var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync();
            
            using var command = new SqliteCommand("SELECT 1", connection);
            await command.ExecuteScalarAsync();
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Connection test failed for domain '{Domain}'", domain);
            return false;
        }
    }
}