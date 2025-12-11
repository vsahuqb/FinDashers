using System.Text.Json;
using FinDashers.Core.Models;

try
{
    // Read and parse the domain configuration
    var jsonContent = File.ReadAllText("../data/domains/payments.json");
    var domainData = JsonSerializer.Deserialize<JsonElement>(jsonContent);
    
    // Create DomainContext from JSON
    var context = new DomainContext
    {
        DomainDescription = domainData.GetProperty("domainDescription").GetString() ?? "",
        SystemPromptInstructions = domainData.GetProperty("systemPromptInstructions").GetString() ?? "",
        UserPrompt = domainData.GetProperty("userPrompt").GetString() ?? "",
        AnnotatedSqlDefinitions = domainData.GetProperty("annotatedSqlDefinitions").GetString() ?? "",
        JoinHints = domainData.GetProperty("joinHints").GetString() ?? "",
        FewShotExamples = domainData.GetProperty("fewShotExamples").GetString() ?? "",
        DatabasePath = domainData.GetProperty("databasePath").GetString() ?? ""
    };
    
    // Parse arrays
    var tableNames = domainData.GetProperty("tableNames").EnumerateArray().Select(x => x.GetString() ?? "").ToList();
    var sqlPreamblePt1 = domainData.GetProperty("sqlPreamblePt1").EnumerateArray().Select(x => x.GetString() ?? "").ToList();
    var sqlPreamblePt2 = domainData.GetProperty("sqlPreamblePt2").EnumerateArray().Select(x => x.GetString() ?? "").ToList();
    
    context.TableNames = tableNames;
    context.SqlPreamblePt1 = sqlPreamblePt1;
    context.SqlPreamblePt2 = sqlPreamblePt2;
    
    Console.WriteLine("✅ Domain configuration loaded successfully!");
    Console.WriteLine($"Domain: {context.DomainDescription}");
    Console.WriteLine($"Tables: {string.Join(", ", context.TableNames)}");
    Console.WriteLine($"Database Path: {context.DatabasePath}");
    Console.WriteLine($"SystemPrompt Length: {context.SystemPrompt.Length} characters");
    Console.WriteLine("\n✅ Task 2.1 validation complete!");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error: {ex.Message}");
}
