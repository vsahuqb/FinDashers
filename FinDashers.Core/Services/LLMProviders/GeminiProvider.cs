using System.Text;
using System.Text.Json;
using FinDashers.Core.Configuration;
using FinDashers.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FinDashers.Core.Services.LLMProviders;

public class GeminiProvider : ILLMProvider
{
    private readonly HttpClient _httpClient;
    private readonly GeminiConfig _config;
    private readonly ILogger<GeminiProvider> _logger;
    
    public string Name => "Gemini";
    
    public GeminiProvider(HttpClient httpClient, IOptions<LLMProvidersConfig> config, ILogger<GeminiProvider> logger)
    {
        _httpClient = httpClient;
        _config = config.Value.Gemini;
        _logger = logger;
        
        _logger.LogInformation("GeminiProvider constructor - ApiKey from config: '{ApiKey}' (Length: {Length})", 
            _config.ApiKey ?? "NULL", _config.ApiKey?.Length ?? 0);
        
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "FinDashers-NL2SQL/1.0");
    }
    
    public async Task<LLMResponse> GenerateSQLAsync(string prompt)
    {
        try
        {
            _logger.LogDebug("Generating SQL using Gemini with prompt length: {Length}", prompt.Length);
            
            var requestBody = new
            {
                contents = new[]
                {
                    new {
                        parts = new[]
                        {
                            new { text = "You are a SQL expert. Generate only SQL queries without explanations.\n\n" + prompt }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.1,
                    maxOutputTokens = 500
                }
            };
            
            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var url = "https://generativelanguage.googleapis.com/v1/models/gemini-pro:generateContent";
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("x-goog-api-key", _config.ApiKey);
            request.Content = content;
            
            var response = await _httpClient.SendAsync(request);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Gemini API error: {StatusCode} - {Content}", response.StatusCode, errorContent);
                throw new HttpRequestException($"Gemini API returned {response.StatusCode}: {errorContent}");
            }
            
            var responseJson = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(responseJson);
            
            var sqlContent = document.RootElement
                .GetProperty("candidates")
                .EnumerateArray()
                .FirstOrDefault()
                .GetProperty("content")
                .GetProperty("parts")
                .EnumerateArray()
                .FirstOrDefault()
                .GetProperty("text")
                .GetString();
            
            _logger.LogInformation("Successfully generated SQL using Gemini");
            
            return new LLMResponse
            {
                SQL = sqlContent?.Trim() ?? string.Empty,
                Provider = Name
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating SQL with Gemini");
            throw;
        }
    }
    
    public async Task<bool> IsAvailableAsync()
    {
        _logger.LogInformation("Checking Gemini availability. API Key configured: {HasKey}, Length: {KeyLength}", 
            !string.IsNullOrEmpty(_config.ApiKey), _config.ApiKey?.Length ?? 0);
        
        if (string.IsNullOrEmpty(_config.ApiKey))
        {
            _logger.LogWarning("Gemini provider not available: API key not configured");
            return false;
        }
        
        // For demo purposes, assume provider is available if API key is configured
        _logger.LogInformation("Gemini provider available with configured API key of length {KeyLength}", _config.ApiKey.Length);
        return true;
    }
}