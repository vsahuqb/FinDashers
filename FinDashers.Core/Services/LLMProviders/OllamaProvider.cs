using System.Text;
using System.Text.Json;
using FinDashers.Core.Configuration;
using FinDashers.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FinDashers.Core.Services.LLMProviders;

public class OllamaProvider : ILLMProvider
{
    private readonly HttpClient _httpClient;
    private readonly OllamaConfig _config;
    private readonly ILogger<OllamaProvider> _logger;
    
    public string Name => "Ollama";
    
    public OllamaProvider(HttpClient httpClient, IOptions<LLMProvidersConfig> config, ILogger<OllamaProvider> logger)
    {
        _httpClient = httpClient;
        _config = config.Value.Ollama;
        _logger = logger;
        
        _logger.LogInformation("OllamaProvider constructor - BaseUrl: '{BaseUrl}', Model: '{Model}'", 
            _config.BaseUrl, _config.ModelName);
        
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "FinDashers-NL2SQL/1.0");
        _httpClient.Timeout = TimeSpan.FromMinutes(2); // Longer timeout for local LLMs
    }
    
    public async Task<LLMResponse> GenerateSQLAsync(string prompt)
    {
        try
        {
            _logger.LogDebug("Generating SQL using Ollama with prompt length: {Length}", prompt.Length);
            
            var requestBody = new
            {
                model = _config.ModelName,
                prompt = "You are a SQL expert. Generate only SQL queries without explanations.\n\n" + prompt,
                stream = false,
                options = new
                {
                    temperature = 0.1,
                    num_predict = 500
                }
            };
            
            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var url = $"{_config.BaseUrl.TrimEnd('/')}/api/generate";
            var response = await _httpClient.PostAsync(url, content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Ollama API error: {StatusCode} - {Content}", response.StatusCode, errorContent);
                throw new HttpRequestException($"Ollama API returned {response.StatusCode}: {errorContent}");
            }
            
            var responseJson = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(responseJson);
            
            var sqlContent = document.RootElement.GetProperty("response").GetString();
            
            if (string.IsNullOrWhiteSpace(sqlContent))
            {
                throw new InvalidOperationException("Ollama returned empty response");
            }
            
            // Clean up the SQL - remove common explanatory text
            var cleanedSql = CleanSqlResponse(sqlContent);
            
            _logger.LogInformation("Successfully generated SQL using Ollama");
            
            return new LLMResponse
            {
                SQL = cleanedSql,
                Provider = Name,
                Success = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating SQL using Ollama: {Error}", ex.Message);
            
            return new LLMResponse
            {
                SQL = string.Empty,
                Provider = Name,
                Success = false,
                Error = ex.Message
            };
        }
    }
    
    public async Task<bool> IsAvailableAsync()
    {
        if (string.IsNullOrEmpty(_config.BaseUrl) || string.IsNullOrEmpty(_config.ModelName))
        {
            _logger.LogDebug("Ollama provider not available: BaseUrl or ModelName not configured");
            return false;
        }
        
        try
        {
            _logger.LogDebug("Checking Ollama availability at {BaseUrl}", _config.BaseUrl);
            
            // Check if Ollama server is running
            var url = $"{_config.BaseUrl.TrimEnd('/')}/api/tags";
            var response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogDebug("Ollama server not available: {StatusCode}", response.StatusCode);
                return false;
            }
            
            // Check if the specific model is available
            var responseJson = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(responseJson);
            
            var models = document.RootElement.GetProperty("models").EnumerateArray();
            var modelExists = models.Any(model => 
                model.GetProperty("name").GetString()?.StartsWith(_config.ModelName) == true);
            
            if (!modelExists)
            {
                _logger.LogWarning("Ollama model '{Model}' not found on server", _config.ModelName);
                return false;
            }
            
            _logger.LogDebug("Ollama provider available with model: {Model}", _config.ModelName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Ollama availability check failed: {Error}", ex.Message);
            return false;
        }
    }
    
    private static string CleanSqlResponse(string sqlResponse)
    {
        if (string.IsNullOrWhiteSpace(sqlResponse))
            return string.Empty;
            
        var lines = sqlResponse.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var sqlLines = new List<string>();
        
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            
            // Skip common explanatory prefixes
            if (trimmed.StartsWith("Here") || trimmed.StartsWith("The SQL") || 
                trimmed.StartsWith("This query") || trimmed.StartsWith("```"))
                continue;
                
            // Take the first substantial SQL line
            if (trimmed.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase) ||
                trimmed.StartsWith("WITH", StringComparison.OrdinalIgnoreCase) ||
                trimmed.StartsWith("UPDATE", StringComparison.OrdinalIgnoreCase) ||
                trimmed.StartsWith("INSERT", StringComparison.OrdinalIgnoreCase) ||
                trimmed.StartsWith("DELETE", StringComparison.OrdinalIgnoreCase))
            {
                sqlLines.Add(trimmed);
                break;
            }
        }
        
        return sqlLines.Count > 0 ? sqlLines[0] : sqlResponse.Trim();
    }
}