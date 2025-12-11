namespace FinDashers.API.Models;

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