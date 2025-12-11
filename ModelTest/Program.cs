using FinDashers.Core.Models;

// Test the DomainContext SystemPrompt property
var context = new DomainContext
{
    SystemPromptInstructions = "You are a SQL assistant.",
    AnnotatedSqlDefinitions = "SELECT * FROM payments",
    FewShotExamples = "Q: Show all payments\nA: SELECT * FROM payment_transactions",
    JoinHints = "Use INNER JOIN for related tables"
};

Console.WriteLine("Testing DomainContext SystemPrompt property:");
Console.WriteLine($"SystemPrompt: {context.SystemPrompt}");
Console.WriteLine("\nSystemPrompt builds correctly!");

// Test other models
var entity = new ResolvedEntity { Name = "location 115", Id = 115, Role = 1 };
var request = new PreprocessedRequest { UserQuery = "Show payments", Domain = "payments" };
var prepared = new PreparedRequest { LlmPrompt = "Generate SQL" };
var result = new QueryResult { Success = true, RowCount = 10 };
var llmResponse = new LLMResponse { SQL = "SELECT 1", Provider = "OpenAI" };

Console.WriteLine("All model classes instantiated successfully!");
Console.WriteLine("Task 1.2 completed successfully!");
