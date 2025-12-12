namespace FinDashers.Core.Models;

public class DomainContext
{
    public string DomainDescription { get; set; } = string.Empty;
    public string SystemPromptInstructions { get; set; } = string.Empty;
    public string UserPrompt { get; set; } = "question: ";
    public string AnnotatedSqlDefinitions { get; set; } = string.Empty;
    public string JoinHints { get; set; } = string.Empty;
    public List<string> TableNames { get; set; } = new();
    public List<string> SqlPreamblePt1 { get; set; } = new();
    public List<string> SqlPreamblePt2 { get; set; } = new();
    public string FewShotExamples { get; set; } = string.Empty;
    public string DatabasePath { get; set; } = string.Empty;
    public string SystemPrompt => BuildSystemPrompt();
    
    private string BuildSystemPrompt()
    {
        return SystemPromptInstructions + 
               (string.IsNullOrEmpty(JoinHints) ? "" : JoinHints + "\n") +
               "<SQL>\n" + AnnotatedSqlDefinitions + "</SQL>\n" + 
               FewShotExamples;
    }
}

public class ResolvedEntity
{
    public string Name { get; set; } = string.Empty;
    public int Id { get; set; }
    public int Role { get; set; }
}

public class PreprocessedRequest
{
    public string UserQuery { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public List<string> NamedResources { get; set; } = new();
}

public class PreparedRequest
{
    public string LlmPrompt { get; set; } = string.Empty;
    public List<string> SqlPreamble { get; set; } = new();
}

public class QueryResult
{
    public bool Success { get; set; }
    public IEnumerable<dynamic> Data { get; set; } = new List<dynamic>();
    public List<string> ColumnNames { get; set; } = new();
    public int RowCount { get; set; }
    public List<string> SqlScript { get; set; } = new();
    public string Error { get; set; } = string.Empty;
}

public class LLMResponse
{
    public string SQL { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public bool Success { get; set; } = true;
    public string? Error { get; set; }
}