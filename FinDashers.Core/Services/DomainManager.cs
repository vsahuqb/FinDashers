using System.Text.Json;
using FinDashers.Core.Models;
using Microsoft.Extensions.Logging;

namespace FinDashers.Core.Services;

public class DomainManager
{
    private readonly Dictionary<string, DomainContext> _domains;
    private readonly ILogger<DomainManager> _logger;
    private readonly string _domainsPath;

    public DomainManager(ILogger<DomainManager> logger, string domainsPath = "data/domains")
    {
        _domains = new Dictionary<string, DomainContext>();
        _logger = logger;
        _domainsPath = domainsPath;
        
        LoadDomains();
    }

    public void LoadDomains()
    {
        try
        {
            _logger.LogInformation("Loading domains from: {DomainsPath}", _domainsPath);
            
            if (!Directory.Exists(_domainsPath))
            {
                _logger.LogWarning("Domains directory not found: {DomainsPath}", _domainsPath);
                return;
            }

            var jsonFiles = Directory.GetFiles(_domainsPath, "*.json");
            
            foreach (var jsonFile in jsonFiles)
            {
                try
                {
                    var domainName = Path.GetFileNameWithoutExtension(jsonFile);
                    var jsonContent = File.ReadAllText(jsonFile);
                    var domainData = JsonSerializer.Deserialize<JsonElement>(jsonContent);
                    
                    var context = new DomainContext
                    {
                        DomainDescription = domainData.GetProperty("domainDescription").GetString() ?? "",
                        SystemPromptInstructions = domainData.GetProperty("systemPromptInstructions").GetString() ?? "",
                        UserPrompt = domainData.GetProperty("userPrompt").GetString() ?? "question: ",
                        AnnotatedSqlDefinitions = domainData.GetProperty("annotatedSqlDefinitions").GetString() ?? "",
                        JoinHints = domainData.GetProperty("joinHints").GetString() ?? "",
                        FewShotExamples = domainData.GetProperty("fewShotExamples").GetString() ?? "",
                        DatabasePath = domainData.GetProperty("databasePath").GetString() ?? ""
                    };
                    
                    // Parse arrays
                    if (domainData.TryGetProperty("tableNames", out var tableNamesElement))
                    {
                        context.TableNames = tableNamesElement.EnumerateArray()
                            .Select(x => x.GetString() ?? "").ToList();
                    }
                    
                    if (domainData.TryGetProperty("sqlPreamblePt1", out var sqlPreamblePt1Element))
                    {
                        context.SqlPreamblePt1 = sqlPreamblePt1Element.EnumerateArray()
                            .Select(x => x.GetString() ?? "").ToList();
                    }
                    
                    if (domainData.TryGetProperty("sqlPreamblePt2", out var sqlPreamblePt2Element))
                    {
                        context.SqlPreamblePt2 = sqlPreamblePt2Element.EnumerateArray()
                            .Select(x => x.GetString() ?? "").ToList();
                    }
                    
                    _domains[domainName] = context;
                    _logger.LogInformation("Loaded domain: {DomainName}", domainName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error loading domain from file: {JsonFile}", jsonFile);
                }
            }
            
            _logger.LogInformation("Successfully loaded {DomainCount} domains", _domains.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading domains");
        }
    }

    public async Task<string> ClassifyDomainAsync(string query)
    {
        await Task.CompletedTask; // Make it async for future enhancements
        
        var lowerQuery = query.ToLowerInvariant();
        
        // Simple keyword-based classification
        // Check for payment-related keywords
        if (lowerQuery.Contains("payment") || lowerQuery.Contains("transaction") || 
            lowerQuery.Contains("revenue") || lowerQuery.Contains("employee") ||
            lowerQuery.Contains("location") || lowerQuery.Contains("store") ||
            lowerQuery.Contains("failed") || lowerQuery.Contains("check") ||
            lowerQuery.Contains("terminal") || lowerQuery.Contains("adyen"))
        {
            if (_domains.ContainsKey("payments"))
            {
                _logger.LogDebug("Classified query as 'payments' domain based on keywords");
                return "payments";
            }
        }
        
        // Future: Add olympic or other domain keywords
        if (lowerQuery.Contains("olympic") || lowerQuery.Contains("athlete") || 
            lowerQuery.Contains("medal") || lowerQuery.Contains("sport"))
        {
            if (_domains.ContainsKey("olympic"))
            {
                _logger.LogDebug("Classified query as 'olympic' domain based on keywords");
                return "olympic";
            }
        }
        
        // Default to first available domain or empty string
        var defaultDomain = _domains.Keys.FirstOrDefault() ?? "";
        _logger.LogDebug("Using default domain: {Domain}", defaultDomain);
        return defaultDomain;
    }

    public async Task<string> BuildPromptAsync(string userQuery, string domain, List<ResolvedEntity> entities)
    {
        await Task.CompletedTask; // Make it async for future enhancements
        
        if (!_domains.TryGetValue(domain, out var domainContext))
        {
            throw new ArgumentException($"Domain '{domain}' not found", nameof(domain));
        }

        // AWS pattern: Build prompt with system instructions, SQL definitions, and few-shot examples
        var prompt = domainContext.SystemPrompt + "\n";
        
        // Add entity context if entities are provided
        if (entities.Any())
        {
            prompt += "\nEntity Context:\n";
            foreach (var entity in entities)
            {
                prompt += $"- {entity.Name}: ID {entity.Id} (role: {entity.Role})\n";
            }
        }
        
        // Add user query with the domain's user prompt prefix
        prompt += $"\n{domainContext.UserPrompt}{userQuery}";
        
        _logger.LogDebug("Built prompt for domain '{Domain}' with {EntityCount} entities, total length: {Length}",
            domain, entities.Count, prompt.Length);
        
        return prompt;
    }

    public List<string> GetSQLPreamble(string domain, List<ResolvedEntity> entities)
    {
        if (!_domains.TryGetValue(domain, out var domainContext))
        {
            throw new ArgumentException($"Domain '{domain}' not found", nameof(domain));
        }

        var preamble = new List<string>();
        
        // Add SQL preamble part 1
        preamble.AddRange(domainContext.SqlPreamblePt1);
        
        // Add identity inserts based on entities (AWS pattern)
        if (entities.Any())
        {
            preamble.Add("-- Identity inserts for resolved entities");
            foreach (var entity in entities)
            {
                // Add entity-specific identity inserts if needed
                preamble.Add($"-- Entity: {entity.Name} (ID: {entity.Id}, Role: {entity.Role})");
            }
        }
        
        // Add SQL preamble part 2
        preamble.AddRange(domainContext.SqlPreamblePt2);
        
        return preamble;
    }

    public DomainContext? GetDomainContext(string domain)
    {
        return _domains.TryGetValue(domain, out var context) ? context : null;
    }

    public List<string> GetAvailableDomains()
    {
        return _domains.Keys.ToList();
    }
}