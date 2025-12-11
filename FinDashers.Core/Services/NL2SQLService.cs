using FinDashers.Core.Models;
using Microsoft.Extensions.Logging;

namespace FinDashers.Core.Services;

public class NL2SQLService
{
    private readonly DomainManager _domainManager;
    private readonly EntityResolver _entityResolver;
    private readonly LLMService _llmService;
    private readonly SQLExecutor _sqlExecutor;
    private readonly ILogger<NL2SQLService> _logger;

    public NL2SQLService(
        DomainManager domainManager,
        EntityResolver entityResolver,
        LLMService llmService,
        SQLExecutor sqlExecutor,
        ILogger<NL2SQLService> logger)
    {
        _domainManager = domainManager;
        _entityResolver = entityResolver;
        _llmService = llmService;
        _sqlExecutor = sqlExecutor;
        _logger = logger;
    }

    public async Task<QueryResult> ProcessAsync(string userQuery, string? domain = null)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("Starting NL2SQL processing for query: '{Query}'", userQuery);

            // Step 1: Preprocess - Domain Classification and Entity Extraction
            var preprocessedRequest = await PreprocessAsync(userQuery, domain);
            _logger.LogDebug("Preprocessing complete. Domain: {Domain}, Entities: {EntityCount}", 
                preprocessedRequest.Domain, preprocessedRequest.NamedResources.Count);

            // Step 2: Resolve Identities - Convert named entities to database IDs
            var resolvedEntities = await _entityResolver.ResolveEntitiesAsync(
                preprocessedRequest.NamedResources, preprocessedRequest.Domain);
            _logger.LogDebug("Entity resolution complete. Resolved: {ResolvedCount}/{TotalCount}", 
                resolvedEntities.Count, preprocessedRequest.NamedResources.Count);

            // Step 3: Prepare - Build LLM prompt and SQL preamble
            var preparedRequest = await PrepareRequestAsync(userQuery, preprocessedRequest.Domain, resolvedEntities);
            _logger.LogDebug("Request preparation complete. Prompt length: {PromptLength}, Preamble statements: {PreambleCount}", 
                preparedRequest.LlmPrompt.Length, preparedRequest.SqlPreamble.Count);

            // Step 4: Generate SQL - Use LLM to create SQL query
            var llmResponse = await _llmService.GenerateSQLAsync(preparedRequest.LlmPrompt);
            _logger.LogInformation("SQL generation complete using provider: {Provider}", llmResponse.Provider);

            // Step 5: Execute Script - Run SQL preamble + generated SQL
            var sqlScript = new List<string>(preparedRequest.SqlPreamble);
            if (!string.IsNullOrEmpty(llmResponse.SQL))
            {
                sqlScript.Add(llmResponse.SQL);
            }

            var queryResult = await _sqlExecutor.ExecuteScriptAsync(sqlScript, preprocessedRequest.Domain);
            
            stopwatch.Stop();
            
            if (queryResult.Success)
            {
                _logger.LogInformation("NL2SQL processing completed successfully in {ElapsedMs}ms. Returned {RowCount} rows", 
                    stopwatch.ElapsedMilliseconds, queryResult.RowCount);
            }
            else
            {
                _logger.LogWarning("NL2SQL processing failed in {ElapsedMs}ms. Error: {Error}", 
                    stopwatch.ElapsedMilliseconds, queryResult.Error);
            }

            return queryResult;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "NL2SQL processing failed after {ElapsedMs}ms: {Error}", 
                stopwatch.ElapsedMilliseconds, ex.Message);

            return new QueryResult
            {
                Success = false,
                Error = ex.Message,
                SqlScript = new List<string> { userQuery }
            };
        }
    }

    private async Task<PreprocessedRequest> PreprocessAsync(string userQuery, string? explicitDomain)
    {
        // Use explicit domain if provided, otherwise classify
        var domain = explicitDomain ?? await _domainManager.ClassifyDomainAsync(userQuery);
        
        // Extract named entities from the query
        var namedResources = await _entityResolver.ExtractEntitiesAsync(userQuery);

        return new PreprocessedRequest
        {
            UserQuery = userQuery,
            Domain = domain,
            NamedResources = namedResources
        };
    }

    private async Task<PreparedRequest> PrepareRequestAsync(string userQuery, string domain, List<ResolvedEntity> entities)
    {
        // Build the LLM prompt using domain context and resolved entities
        var llmPrompt = await _domainManager.BuildPromptAsync(userQuery, domain, entities);
        
        // Get SQL preamble with identity inserts for resolved entities
        var sqlPreamble = _domainManager.GetSQLPreamble(domain, entities);

        return new PreparedRequest
        {
            LlmPrompt = llmPrompt,
            SqlPreamble = sqlPreamble
        };
    }

    public async Task<List<string>> GetAvailableDomainsAsync()
    {
        return await Task.FromResult(_domainManager.GetAvailableDomains());
    }

    public async Task<List<string>> GetAvailableProvidersAsync()
    {
        return await _llmService.GetAvailableProvidersAsync();
    }

    public async Task<bool> TestDomainConnectionAsync(string domain)
    {
        return await _sqlExecutor.TestConnectionAsync(domain);
    }
}