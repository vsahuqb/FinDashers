using FinDashers.API.Models;
using FinDashers.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinDashers.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QueryController : ControllerBase
{
    private readonly NL2SQLService _nl2SqlService;
    private readonly ILogger<QueryController> _logger;

    // Test cases for demo purposes
    private static readonly string[] TestQueries = {
        "Show me all payments for location 115 today",
        "What's the total revenue processed by employee 3836?", 
        "How many failed payments do we have?",
        "Show me all Mastercard transactions",
        "What's the average transaction amount for store 2054?",
        "List all payments for check ID 692cb24db93afd5132436601",
        "Which terminal processed the most transactions?",
        "Show me refunded payments this week"
    };

    public QueryController(NL2SQLService nl2SqlService, ILogger<QueryController> logger)
    {
        _nl2SqlService = nl2SqlService;
        _logger = logger;
    }

    [HttpPost("query")]
    public async Task<IActionResult> ProcessQuery([FromBody] QueryRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Query))
            {
                return BadRequest(new { error = "Query cannot be empty" });
            }

            _logger.LogInformation("Processing query: '{Query}' for domain: '{Domain}'", 
                request.Query, request.Domain ?? "auto-detect");

            var result = await _nl2SqlService.ProcessAsync(request.Query, request.Domain);

            var response = new QueryResponse
            {
                Success = result.Success,
                Data = result.Data,
                ColumnNames = result.ColumnNames,
                RowCount = result.RowCount,
                SqlScript = result.SqlScript,
                Error = result.Error
            };

            if (result.Success)
            {
                _logger.LogInformation("Query processed successfully, returned {RowCount} rows", result.RowCount);
                return Ok(response);
            }
            else
            {
                _logger.LogWarning("Query processing failed: {Error}", result.Error);
                return BadRequest(response);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing query: {Error}", ex.Message);
            return StatusCode(500, new QueryResponse
            {
                Success = false,
                Error = ex.Message
            });
        }
    }

    [HttpGet("domains")]
    public async Task<IActionResult> GetDomains()
    {
        try
        {
            var domains = await _nl2SqlService.GetAvailableDomainsAsync();
            var providers = await _nl2SqlService.GetAvailableProvidersAsync();

            var response = new DomainsResponse
            {
                AvailableDomains = domains,
                AvailableProviders = providers
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting domains: {Error}", ex.Message);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("test/{index}")]
    public async Task<IActionResult> RunTestQuery(int index)
    {
        try
        {
            if (index < 0 || index >= TestQueries.Length)
            {
                return BadRequest(new { error = $"Test index must be between 0 and {TestQueries.Length - 1}" });
            }

            var testQuery = TestQueries[index];
            _logger.LogInformation("Running test query {Index}: '{Query}'", index, testQuery);

            var result = await _nl2SqlService.ProcessAsync(testQuery);

            var response = new TestQueryResponse
            {
                TestIndex = index,
                TestQuery = testQuery,
                Success = result.Success,
                Data = result.Data,
                ColumnNames = result.ColumnNames,
                RowCount = result.RowCount,
                SqlScript = result.SqlScript,
                Error = result.Error
            };

            if (result.Success)
            {
                return Ok(response);
            }
            else
            {
                return BadRequest(response);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running test query {Index}: {Error}", index, ex.Message);
            return StatusCode(500, new TestQueryResponse
            {
                TestIndex = index,
                TestQuery = index < TestQueries.Length ? TestQueries[index] : "",
                Success = false,
                Error = ex.Message
            });
        }
    }

    [HttpGet("test")]
    public IActionResult GetTestQueries()
    {
        var testQueries = TestQueries.Select((query, index) => new
        {
            Index = index,
            Query = query
        }).ToArray();

        return Ok(new { TestQueries = testQueries });
    }

    [HttpGet("health")]
    public async Task<IActionResult> GetHealth()
    {
        try
        {
            var domains = await _nl2SqlService.GetAvailableDomainsAsync();
            var providers = await _nl2SqlService.GetAvailableProvidersAsync();

            var health = new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                AvailableDomains = domains,
                AvailableProviders = providers,
                Version = "1.0.0"
            };

            return Ok(health);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed: {Error}", ex.Message);
            return StatusCode(503, new
            {
                Status = "Unhealthy",
                Error = ex.Message,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}