using System.Text;
using System.Text.Json;
using FinDashers.Core.Configuration;
using FinDashers.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FinDashers.Core.Services.LLMProviders;

public class AnthropicProvider : ILLMProvider
{
    private readonly HttpClient _httpClient;
    private readonly AnthropicConfig _config;
    private readonly ILogger<AnthropicProvider> _logger;
    
    public string Name => "Anthropic";
    
    public AnthropicProvider(HttpClient httpClient, IOptions<LLMProvidersConfig> config, ILogger<AnthropicProvider> logger)
    {
        _httpClient = httpClient;
        _config = config.Value.Anthropic;
        _logger = logger;
        
        _httpClient.DefaultRequestHeaders.Add("x-api-key", _config.ApiKey);
        _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "FinDashers-NL2SQL/1.0");
    }
    
    public async Task<LLMResponse> GenerateSQLAsync(string prompt)
    {
        try
        {
            _logger.LogDebug("Generating SQL using Anthropic with prompt length: {Length}", prompt.Length);
            
            var requestBody = new
            {
                model = "claude-3-haiku-20240307",
                max_tokens = 500,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };
            
            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("https://api.anthropic.com/v1/messages", content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Anthropic API error: {StatusCode} - {Content}", response.StatusCode, errorContent);
                throw new HttpRequestException($"Anthropic API returned {response.StatusCode}: {errorContent}");
            }
            
            var responseJson = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(responseJson);
            
            var sqlContent = document.RootElement
                .GetProperty("content")
                .EnumerateArray()
                .FirstOrDefault()
                .GetProperty("text")
                .GetString();
            
            _logger.LogInformation("Successfully generated SQL using Anthropic");
            
            return new LLMResponse
            {
                SQL = sqlContent?.Trim() ?? string.Empty,
                Provider = Name
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating SQL with Anthropic");
            throw;
        }
    }
    
    public async Task<bool> IsAvailableAsync()
    {
        if (string.IsNullOrEmpty(_config.ApiKey))
        {
            _logger.LogDebug("Anthropic provider not available: API key not configured");
            return false;
        }
        
        try
        {
            // Simple health check - try to get account info or make a minimal request
            var testBody = new
            {
                model = "claude-3-haiku-20240307",
                max_tokens = 10,
                messages = new[]
                {
                    new { role = "user", content = "test" }
                }
            };
            
            var json = JsonSerializer.Serialize(testBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("https://api.anthropic.com/v1/messages", content);
            var isAvailable = response.IsSuccessStatusCode;
            
            _logger.LogDebug("Anthropic provider availability check: {Available}", isAvailable);
            return isAvailable;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Anthropic provider availability check failed");
            return false;
        }
    }
}