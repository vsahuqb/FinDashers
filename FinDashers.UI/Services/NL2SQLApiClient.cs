using System.Text;
using System.Text.Json;
using FinDashers.Core.Models;

namespace FinDashers.UI.Services;

public class NL2SQLApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<NL2SQLApiClient> _logger;
    private readonly JsonSerializerOptions _jsonOptions;


    public NL2SQLApiClient(IHttpClientFactory httpClientFactory, ILogger<NL2SQLApiClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<QueryResponse> ProcessQueryAsync(string query, string? domain = null)
    {
        using var httpClient = _httpClientFactory.CreateClient("FinDashersApi");
        
        try
        {
            var request = new QueryRequest
            {
                Query = query,
                Domain = domain
            };

            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("api/query/query", content);
            var responseJson = await response.Content.ReadAsStringAsync();

            _logger.LogDebug("API Response: Status={StatusCode}, Content={Content}", response.StatusCode, responseJson);

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<QueryResponse>(responseJson, _jsonOptions);
                return result ?? new QueryResponse { Success = false, Error = "Failed to deserialize response" };
            }
            else
            {
                var errorResult = JsonSerializer.Deserialize<QueryResponse>(responseJson, _jsonOptions);
                return errorResult ?? new QueryResponse { Success = false, Error = $"HTTP {response.StatusCode}" };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing query: {Query}", query);
            return new QueryResponse
            {
                Success = false,
                Error = ex.Message
            };
        }
    }

    public async Task<DomainsResponse> GetDomainsAsync()
    {
        using var httpClient = _httpClientFactory.CreateClient("FinDashersApi");
        
        try
        {
            var response = await httpClient.GetAsync("api/query/domains");
            var json = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<DomainsResponse>(json, _jsonOptions);
                return result ?? new DomainsResponse();
            }
            else
            {
                _logger.LogWarning("Failed to get domains: {StatusCode}", response.StatusCode);
                return new DomainsResponse();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting domains");
            return new DomainsResponse();
        }
    }

    public async Task<TestQueryResponse> RunTestQueryAsync(int index)
    {
        using var httpClient = _httpClientFactory.CreateClient("FinDashersApi");
        
        try
        {
            var response = await httpClient.GetAsync($"api/query/test/{index}");
            var json = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<TestQueryResponse>(json, _jsonOptions);
                return result ?? new TestQueryResponse { Success = false, Error = "Failed to deserialize response" };
            }
            else
            {
                var errorResult = JsonSerializer.Deserialize<TestQueryResponse>(json, _jsonOptions);
                return errorResult ?? new TestQueryResponse { Success = false, Error = $"HTTP {response.StatusCode}" };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running test query {Index}", index);
            return new TestQueryResponse
            {
                Success = false,
                Error = ex.Message,
                TestIndex = index
            };
        }
    }
}

public class QueryRequest
{
    public string Query { get; set; } = string.Empty;
    public string? Domain { get; set; }
}

public class QueryResponse
{
    public bool Success { get; set; }
    public IEnumerable<dynamic>? Data { get; set; }
    public List<string> ColumnNames { get; set; } = new();
    public int RowCount { get; set; }
    public List<string> SqlScript { get; set; } = new();
    public string? Error { get; set; }
    public string? Provider { get; set; }
}

public class DomainsResponse
{
    public List<string> AvailableDomains { get; set; } = new();
    public List<string> AvailableProviders { get; set; } = new();
}

public class TestQueryResponse : QueryResponse
{
    public string TestQuery { get; set; } = string.Empty;
    public int TestIndex { get; set; }
}