using FinDashers.Core.Models;

namespace FinDashers.Core.Services;

public interface ILLMProvider
{
    string Name { get; }
    Task<LLMResponse> GenerateSQLAsync(string prompt);
    Task<bool> IsAvailableAsync();
}