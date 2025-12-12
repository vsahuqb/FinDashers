using System.Text.Json;
using System.Text.RegularExpressions;
using FinDashers.Core.Models;
using Microsoft.Extensions.Logging;

namespace FinDashers.Core.Services;

public class EntityResolver
{
    private readonly Dictionary<string, Dictionary<string, JsonElement>> _entityMappings;
    private readonly ILogger<EntityResolver> _logger;
    private readonly string _entitiesPath;

    public EntityResolver(ILogger<EntityResolver> logger, string entitiesPath = "data/entities")
    {
        _entityMappings = new Dictionary<string, Dictionary<string, JsonElement>>();
        _logger = logger;
        _entitiesPath = entitiesPath;
        
        LoadEntityMappings();
    }

    private void LoadEntityMappings()
    {
        try
        {
            _logger.LogInformation("Loading entity mappings from: {EntitiesPath}", _entitiesPath);
            
            if (!Directory.Exists(_entitiesPath))
            {
                _logger.LogWarning("Entities directory not found: {EntitiesPath}", _entitiesPath);
                return;
            }

            var jsonFiles = Directory.GetFiles(_entitiesPath, "*.json");
            
            foreach (var jsonFile in jsonFiles)
            {
                try
                {
                    var fileName = Path.GetFileNameWithoutExtension(jsonFile);
                    var jsonContent = File.ReadAllText(jsonFile);
                    var entityData = JsonSerializer.Deserialize<JsonElement>(jsonContent);
                    
                    // Load entity mappings for each domain
                    foreach (var domainProperty in entityData.EnumerateObject())
                    {
                        var domainName = domainProperty.Name;
                        var entities = new Dictionary<string, JsonElement>();
                        
                        foreach (var entityProperty in domainProperty.Value.EnumerateObject())
                        {
                            entities[entityProperty.Name.ToLowerInvariant()] = entityProperty.Value;
                        }
                        
                        _entityMappings[domainName] = entities;
                        _logger.LogInformation("Loaded {EntityCount} entities for domain: {DomainName}", 
                            entities.Count, domainName);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error loading entities from file: {JsonFile}", jsonFile);
                }
            }
            
            _logger.LogInformation("Successfully loaded entity mappings for {DomainCount} domains", 
                _entityMappings.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading entity mappings");
        }
    }

    public async Task<List<string>> ExtractEntitiesAsync(string query)
    {
        await Task.CompletedTask;
        
        var entities = new List<string>();
        var lowerQuery = query.ToLowerInvariant();
        
        // AWS pattern: Extract "firstname lastname" pairs
        var namePattern = @"\b([a-z]+)\s+([a-z]+)\b";
        var nameMatches = Regex.Matches(lowerQuery, namePattern, RegexOptions.IgnoreCase);
        
        foreach (Match match in nameMatches)
        {
            var fullName = match.Value.ToLowerInvariant();
            if (IsNamedResource(fullName))
            {
                entities.Add(fullName);
            }
        }
        
        // Extract specific entity patterns
        var entityPatterns = new[]
        {
            @"\blocation\s+(\d+)\b",
            @"\bemployee\s+(\d+)\b", 
            @"\bterminal\s+([a-z0-9\-]+)\b",
            @"\bcompany\s+(\d+)\b",
            @"\bstore\s+(\d+)\b",
            @"\bcheck\s+id\s+([a-f0-9]+)\b",
            @"\b(visa|mastercard|amex|american\s+express)\b"
        };
        
        foreach (var pattern in entityPatterns)
        {
            var matches = Regex.Matches(lowerQuery, pattern, RegexOptions.IgnoreCase);
            foreach (Match match in matches)
            {
                entities.Add(match.Value.ToLowerInvariant());
            }
        }
        
        var uniqueEntities = entities.Distinct().ToList();
        
        _logger.LogDebug("Extracted {EntityCount} entities from query: {Entities}", 
            uniqueEntities.Count, string.Join(", ", uniqueEntities));
        
        return uniqueEntities;
    }

    public async Task<List<ResolvedEntity>> ResolveEntitiesAsync(List<string> entities, string domain)
    {
        await Task.CompletedTask;
        
        var resolvedEntities = new List<ResolvedEntity>();
        
        if (!_entityMappings.TryGetValue(domain, out var domainEntities))
        {
            _logger.LogWarning("No entity mappings found for domain: {Domain}", domain);
            return resolvedEntities;
        }
        
        foreach (var entityName in entities)
        {
            var lowerEntityName = entityName.ToLowerInvariant();
            
            if (domainEntities.TryGetValue(lowerEntityName, out var entityData))
            {
                try
                {
                    var id = entityData.GetProperty("id").GetString() ?? "";
                    var roleStr = entityData.GetProperty("role").GetString() ?? "";
                    
                    var role = int.TryParse(roleStr, out var roleInt) ? roleInt : 0;
                    
                    var resolvedEntity = new ResolvedEntity
                    {
                        Name = entityName,
                        Id = int.TryParse(id, out var idInt) ? idInt : 0,
                        Role = role
                    };
                    
                    resolvedEntities.Add(resolvedEntity);
                    _logger.LogDebug("Resolved entity '{EntityName}' to ID: {Id}, Role: {Role}", 
                        entityName, resolvedEntity.Id, resolvedEntity.Role);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error resolving entity: {EntityName}", entityName);
                }
            }
            else
            {
                _logger.LogDebug("Entity '{EntityName}' not found in domain '{Domain}' mappings", 
                    entityName, domain);
            }
        }
        
        _logger.LogInformation("Resolved {ResolvedCount} out of {TotalCount} entities for domain '{Domain}'", 
            resolvedEntities.Count, entities.Count, domain);
        
        return resolvedEntities;
    }

    public bool IsNamedResource(string candidate)
    {
        var lowerCandidate = candidate.ToLowerInvariant();
        
        // Check against all domain mappings
        foreach (var domainEntities in _entityMappings.Values)
        {
            if (domainEntities.ContainsKey(lowerCandidate))
            {
                return true;
            }
        }
        
        // Check for common entity patterns
        var entityPatterns = new[]
        {
            @"^location\s+\d+$",
            @"^employee\s+\d+$",
            @"^terminal\s+[a-z0-9\-]+$",
            @"^company\s+\d+$", 
            @"^store\s+\d+$"
        };
        
        return entityPatterns.Any(pattern => 
            Regex.IsMatch(lowerCandidate, pattern, RegexOptions.IgnoreCase));
    }

    public Dictionary<string, int> GetEntityCounts()
    {
        return _entityMappings.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.Count
        );
    }
}