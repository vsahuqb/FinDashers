namespace FinDashers.Core.Configuration;

public class LLMProvidersConfig
{
    public GeminiConfig Gemini { get; set; } = new();
    public OpenAIConfig OpenAI { get; set; } = new();
    public AnthropicConfig Anthropic { get; set; } = new();
    public AzureOpenAIConfig AzureOpenAI { get; set; } = new();
    public OllamaConfig Ollama { get; set; } = new();
}

public class OpenAIConfig
{
    public string ApiKey { get; set; } = string.Empty;
}

public class AnthropicConfig
{
    public string ApiKey { get; set; } = string.Empty;
}

public class AzureOpenAIConfig
{
    public string ApiKey { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string DeploymentName { get; set; } = "gpt-4";
}

public class GeminiConfig
{
    public string ApiKey { get; set; } = string.Empty;
}

public class OllamaConfig
{
    public string BaseUrl { get; set; } = "http://localhost:11434";
    public string ModelName { get; set; } = "llama3.2";
}

public class DatabaseConfig
{
    public string Payments { get; set; } = string.Empty;
    public string Adyen { get; set; } = string.Empty;
}