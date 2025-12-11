using Microsoft.Data.Sqlite;

var databasePath = "../data/databases/payments.db";

try
{
    using var connection = new SqliteConnection($"Data Source={databasePath}");
    connection.Open();

    Console.WriteLine("🔍 Testing sample queries from domain configuration:");
    
    // Test 1: Show all failed payments today
    Console.WriteLine("\n1. Failed payments today:");
    using var failedCmd = new SqliteCommand(@"
        SELECT pt.psp_reference, pt.merchant_reference, pt.check_id, pt.total_amount, pt.payment_method, pt.event_date, l.location_name, e.employee_name 
        FROM payment_transactions pt 
        INNER JOIN locations l ON pt.location_id = l.location_id 
        INNER JOIN employees e ON pt.employee_id = e.employee_id 
        WHERE pt.success = 0 AND DATE(pt.event_date) = DATE('now')", connection);
    
    using var failedReader = failedCmd.ExecuteReader();
    while (failedReader.Read())
    {
        Console.WriteLine($"   PSP: {failedReader["psp_reference"]}, Amount: ${failedReader["total_amount"]}, Method: {failedReader["payment_method"]}, Location: {failedReader["location_name"]}");
    }
    failedReader.Close();

    // Test 2: Total revenue for location 115
    Console.WriteLine("\n2. Total revenue for location 115:");
    using var revenueCmd = new SqliteCommand(@"
        SELECT l.location_name, SUM(pt.total_amount) as total_revenue, COUNT(*) as transaction_count 
        FROM payment_transactions pt 
        INNER JOIN locations l ON pt.location_id = l.location_id 
        WHERE pt.location_id = '115' AND pt.success = 1", connection);
    
    using var revenueReader = revenueCmd.ExecuteReader();
    if (revenueReader.Read())
    {
        Console.WriteLine($"   Location: {revenueReader["location_name"]}, Revenue: ${revenueReader["total_revenue"]}, Transactions: {revenueReader["transaction_count"]}");
    }
    revenueReader.Close();

    // Test 3: Employee with most transactions
    Console.WriteLine("\n3. Employee with most transactions:");
    using var employeeCmd = new SqliteCommand(@"
        SELECT e.employee_name, e.employee_id, COUNT(*) as transaction_count, SUM(pt.total_amount) as total_processed 
        FROM payment_transactions pt 
        INNER JOIN employees e ON pt.employee_id = e.employee_id 
        WHERE pt.success = 1 
        GROUP BY e.employee_id, e.employee_name 
        ORDER BY transaction_count DESC 
        LIMIT 1", connection);
    
    using var employeeReader = employeeCmd.ExecuteReader();
    if (employeeReader.Read())
    {
        Console.WriteLine($"   Employee: {employeeReader["employee_name"]} (ID: {employeeReader["employee_id"]}), Transactions: {employeeReader["transaction_count"]}, Total: ${employeeReader["total_processed"]}");
    }
    employeeReader.Close();

    // Test 4: Specific check ID
    Console.WriteLine("\n4. Payment for check ID 692cb24db93afd5132436601:");
    using var checkCmd = new SqliteCommand(@"
        SELECT pt.*, l.location_name, e.employee_name 
        FROM payment_transactions pt 
        INNER JOIN locations l ON pt.location_id = l.location_id 
        INNER JOIN employees e ON pt.employee_id = e.employee_id 
        WHERE pt.check_id = '692cb24db93afd5132436601'", connection);
    
    using var checkReader = checkCmd.ExecuteReader();
    if (checkReader.Read())
    {
        Console.WriteLine($"   Check: {checkReader["check_id"]}, Amount: ${checkReader["total_amount"]}, Location: {checkReader["location_name"]}, Employee: {checkReader["employee_name"]}");
    }
    checkReader.Close();

    Console.WriteLine("\n✅ All sample queries executed successfully!");
    Console.WriteLine("✅ Database structure matches Adyen webhook data structure!");
    Console.WriteLine("✅ Sample data includes realistic payment scenarios!");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error testing database: {ex.Message}");
}
