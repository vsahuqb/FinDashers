using FinDashers.Core.Models;
using Microsoft.Extensions.Logging;

namespace FinDashers.Core.Services;

public class LLMService
{
    private readonly List<ILLMProvider> _providers;
    private readonly ILogger<LLMService> _logger;
    
    public LLMService(IEnumerable<ILLMProvider> providers, ILogger<LLMService> logger)
    {
        _providers = providers.ToList();
        _logger = logger;
        
        _logger.LogInformation("LLMService initialized with {ProviderCount} providers: {Providers}", 
            _providers.Count, string.Join(", ", _providers.Select(p => p.Name)));
    }
    
    public async Task<LLMResponse> GenerateSQLAsync(string prompt)
    {
        if (_providers.Count == 0)
        {
            throw new InvalidOperationException("No LLM providers configured");
        }
        
        var exceptions = new List<Exception>();
        
        foreach (var provider in _providers)
        {
            try
            {
                _logger.LogDebug("Attempting SQL generation with provider: {Provider}", provider.Name);
                
                // Check if provider is available first
                var isAvailable = await provider.IsAvailableAsync();
                if (!isAvailable)
                {
                    _logger.LogWarning("Provider {Provider} is not available, skipping", provider.Name);
                    continue;
                }
                
                // Try to generate SQL
                var response = await provider.GenerateSQLAsync(prompt);
                
                _logger.LogInformation("Successfully generated SQL using provider: {Provider}", provider.Name);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Provider {Provider} failed to generate SQL: {Error}", 
                    provider.Name, ex.Message);
                exceptions.Add(ex);
            }
        }
        
        // All providers failed
        var aggregateException = new AggregateException(
            "All LLM providers failed to generate SQL", exceptions);
        
        _logger.LogError(aggregateException, "All {ProviderCount} providers failed to generate SQL", 
            _providers.Count);
        
        throw aggregateException;
    }
    
    public async Task<List<string>> GetAvailableProvidersAsync()
    {
        _logger.LogInformation("Checking availability for {ProviderCount} providers", _providers.Count);
        var availableProviders = new List<string>();
        
        foreach (var provider in _providers)
        {
            try
            {
                _logger.LogInformation("Checking availability for provider: {Provider}", provider.Name);
                var isAvailable = await provider.IsAvailableAsync();
                _logger.LogInformation("Provider {Provider} availability: {Available}", provider.Name, isAvailable);
                if (isAvailable)
                {
                    availableProviders.Add(provider.Name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking availability for provider {Provider}: {Error}", provider.Name, ex.Message);
            }
        }
        
        _logger.LogInformation("Found {AvailableCount} available providers: {Providers}", 
            availableProviders.Count, string.Join(", ", availableProviders));
        return availableProviders;
    }
}