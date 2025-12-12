using System.Text;
using System.Text.Json;
using FinDashers.Core.Configuration;
using FinDashers.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FinDashers.Core.Services.LLMProviders;

public class OpenAIProvider : ILLMProvider
{
    private readonly HttpClient _httpClient;
    private readonly OpenAIConfig _config;
    private readonly ILogger<OpenAIProvider> _logger;
    
    public string Name => "OpenAI";
    
    public OpenAIProvider(HttpClient httpClient, IOptions<LLMProvidersConfig> config, ILogger<OpenAIProvider> logger)
    {
        _httpClient = httpClient;
        _config = config.Value.OpenAI;
        _logger = logger;
        
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config.ApiKey}");
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "FinDashers-NL2SQL/1.0");
    }
    
    public async Task<LLMResponse> GenerateSQLAsync(string prompt)
    {
        try
        {
            _logger.LogDebug("Generating SQL using OpenAI with prompt length: {Length}", prompt.Length);
            
            var requestBody = new
            {
                model = "gpt-4",
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
            
            var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("OpenAI API error: {StatusCode} - {Content}", response.StatusCode, errorContent);
                throw new HttpRequestException($"OpenAI API returned {response.StatusCode}: {errorContent}");
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
            
            _logger.LogInformation("Successfully generated SQL using OpenAI");
            
            var cleanedSql = ExtractSqlFromResponse(sqlContent?.Trim() ?? string.Empty);
            
            return new LLMResponse
            {
                SQL = cleanedSql,
                Provider = Name
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating SQL with OpenAI");
            throw;
        }
    }
    
    private static string ExtractSqlFromResponse(string response)
    {
        if (string.IsNullOrWhiteSpace(response))
            return response;

        // Remove "A: " prefix if present
        if (response.StartsWith("A: ", StringComparison.OrdinalIgnoreCase))
        {
            return response.Substring(3).Trim();
        }

        return response;
    }
    
    public async Task<bool> IsAvailableAsync()
    {
        if (string.IsNullOrEmpty(_config.ApiKey))
        {
            _logger.LogDebug("OpenAI provider not available: API key not configured");
            return false;
        }
        return true;
        
        //try
        //{
        //    // Simple health check - get models list
        //    var response = await _httpClient.GetAsync("https://api.openai.com/v1/models");
        //    var isAvailable = response.IsSuccessStatusCode;
            
        //    _logger.LogDebug("OpenAI provider availability check: {Available}", isAvailable);
        //    return isAvailable;
        //}
        //catch (Exception ex)
        //{
        //    _logger.LogDebug(ex, "OpenAI provider availability check failed");
        //    return false;
        //}
    }
}