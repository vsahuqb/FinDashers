using System.Text;
using System.Text.Json;
using FinDashers.Core.Configuration;
using FinDashers.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FinDashers.Core.Services.LLMProviders;

public class AzureOpenAIProvider : ILLMProvider
{
    private readonly HttpClient _httpClient;
    private readonly AzureOpenAIConfig _config;
    private readonly ILogger<AzureOpenAIProvider> _logger;
    
    public string Name => "AzureOpenAI";
    
    public AzureOpenAIProvider(HttpClient httpClient, IOptions<LLMProvidersConfig> config, ILogger<AzureOpenAIProvider> logger)
    {
        _httpClient = httpClient;
        _config = config.Value.AzureOpenAI;
        _logger = logger;
        
        _httpClient.DefaultRequestHeaders.Add("api-key", _config.ApiKey);
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "FinDashers-NL2SQL/1.0");
    }
    
    public async Task<LLMResponse> GenerateSQLAsync(string prompt)
    {
        try
        {
            _logger.LogDebug("Generating SQL using Azure OpenAI with prompt length: {Length}", prompt.Length);
            
            var requestBody = new
            {
                messages = new[]
                {
                    new { role = "system", content = "You are a SQL expert. Generate only SQL queries without explanations." },
                    new { role = "user", content = prompt }
                },
                max_tokens = 500,
                temperature = 0.1
            };
            
            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var url = $"{_config.Endpoint.TrimEnd('/')}/openai/deployments/{_config.DeploymentName}/chat/completions?api-version=2023-12-01-preview";
            var response = await _httpClient.PostAsync(url, content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Azure OpenAI API error: {StatusCode} - {Content}", response.StatusCode, errorContent);
                throw new HttpRequestException($"Azure OpenAI API returned {response.StatusCode}: {errorContent}");
            }
            
            var responseJson = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(responseJson);
            
            var sqlContent = document.RootElement
                .GetProperty("choices")
                .EnumerateArray()
                .FirstOrDefault()
                .GetProperty("message")
                .GetProperty("content")
                .GetString();
            
            _logger.LogInformation("Successfully generated SQL using Azure OpenAI");
            
            return new LLMResponse
            {
                SQL = sqlContent?.Trim() ?? string.Empty,
                Provider = Name
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating SQL with Azure OpenAI");
            throw;
        }
    }
    
    public async Task<bool> IsAvailableAsync()
    {
        if (string.IsNullOrEmpty(_config.ApiKey) || string.IsNullOrEmpty(_config.Endpoint))
        {
            _logger.LogDebug("Azure OpenAI provider not available: API key or endpoint not configured");
            return false;
        }
        
        try
        {
            // Simple health check
            var url = $"{_config.Endpoint.TrimEnd('/')}/openai/deployments?api-version=2023-12-01-preview";
            var response = await _httpClient.GetAsync(url);
            var isAvailable = response.IsSuccessStatusCode;
            
            _logger.LogDebug("Azure OpenAI provider availability check: {Available}", isAvailable);
            return isAvailable;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Azure OpenAI provider availability check failed");
            return false;
        }
    }
}