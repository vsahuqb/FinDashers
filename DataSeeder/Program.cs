using Npgsql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;

var connectionString = "Host=localhost;Port=5432;Database=adyen_db;Username=postgres;Password=admin;Pooling=true;";
var csvPath = "../FinDashers.API/adyen_webhook_dummy_10000_fixed.csv";

try
{
    using var connection = new NpgsqlConnection(connectionString);
    await connection.OpenAsync();
    
    // Create table if not exists
    var createTableSql = @"
        CREATE TABLE IF NOT EXISTS adyen_transactions (
            id BIGSERIAL PRIMARY KEY,
            psp_reference VARCHAR(255) NOT NULL,
            merchant_reference VARCHAR(255),
            event_code VARCHAR(100) NOT NULL,
            event_date TIMESTAMP NOT NULL,
            approved_amount DECIMAL(10,2) NOT NULL DEFAULT 0,
            currency VARCHAR(10) NOT NULL DEFAULT 'USD',
            merchant_account VARCHAR(255),
            payment_method VARCHAR(100),
            reason VARCHAR(255),
            success BOOLEAN NOT NULL DEFAULT false,
            location_id VARCHAR(100),
            company_id INTEGER,
            terminal_id VARCHAR(100),
            tender_reference VARCHAR(255),
            raw_event JSONB,
            created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
        );
        
        CREATE INDEX IF NOT EXISTS idx_adyen_transactions_event_code ON adyen_transactions(event_code);
        CREATE INDEX IF NOT EXISTS idx_adyen_transactions_event_date ON adyen_transactions(event_date);
        CREATE INDEX IF NOT EXISTS idx_adyen_transactions_location_id ON adyen_transactions(location_id);
        CREATE INDEX IF NOT EXISTS idx_adyen_transactions_success ON adyen_transactions(success);
        CREATE INDEX IF NOT EXISTS idx_adyen_transactions_payment_method ON adyen_transactions(payment_method);
    ";
    
    using var createCmd = new NpgsqlCommand(createTableSql, connection);
    await createCmd.ExecuteNonQueryAsync();
    
    // Clear existing data
    using var clearCmd = new NpgsqlCommand("TRUNCATE TABLE adyen_transactions;", connection);
    await clearCmd.ExecuteNonQueryAsync();
    
    // Read and import CSV
    var lines = await File.ReadAllLinesAsync(csvPath);
    var insertSql = @"
        INSERT INTO adyen_transactions (
            psp_reference, merchant_reference, event_code, event_date, approved_amount,
            currency, merchant_account, payment_method, reason, success, location_id,
            company_id, terminal_id, tender_reference, raw_event, created_at
        ) VALUES (
            @psp_reference, @merchant_reference, @event_code, @event_date, @approved_amount,
            @currency, @merchant_account, @payment_method, @reason, @success, @location_id,
            @company_id, @terminal_id, @tender_reference, @raw_event::jsonb, @created_at
        )";
    
    var count = 0;
    for (int i = 1; i < lines.Length; i++) // Skip header
    {
        var line = lines[i];
        if (string.IsNullOrWhiteSpace(line)) continue;
        
        var fields = ParseCsvLine(line);
        if (fields.Length < 16) continue;
        
        using var cmd = new NpgsqlCommand(insertSql, connection);
        cmd.Parameters.AddWithValue("@psp_reference", fields[0]);
        cmd.Parameters.AddWithValue("@merchant_reference", fields[1]);
        cmd.Parameters.AddWithValue("@event_code", fields[2]);
        cmd.Parameters.AddWithValue("@event_date", DateTime.Parse(fields[3]));
        cmd.Parameters.AddWithValue("@approved_amount", decimal.Parse(fields[4]));
        cmd.Parameters.AddWithValue("@currency", fields[5]);
        cmd.Parameters.AddWithValue("@merchant_account", fields[6]);
        cmd.Parameters.AddWithValue("@payment_method", fields[7]);
        cmd.Parameters.AddWithValue("@reason", fields[8]);
        cmd.Parameters.AddWithValue("@success", bool.Parse(fields[9]));
        cmd.Parameters.AddWithValue("@location_id", fields[10]);
        cmd.Parameters.AddWithValue("@company_id", int.Parse(fields[11]));
        cmd.Parameters.AddWithValue("@terminal_id", fields[12]);
        cmd.Parameters.AddWithValue("@tender_reference", fields[13]);
        cmd.Parameters.AddWithValue("@raw_event", fields[14]);
        cmd.Parameters.AddWithValue("@created_at", DateTime.Parse(fields[15]));
        
        await cmd.ExecuteNonQueryAsync();
        count++;
        
        if (count % 1000 == 0)
        {
            Console.WriteLine($"Imported {count} records...");
        }
    }
    
    Console.WriteLine($"✅ Successfully imported {count} records from CSV!");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
}

static string[] ParseCsvLine(string line)
{
    var fields = new List<string>();
    var inQuotes = false;
    var currentField = "";
    
    for (int i = 0; i < line.Length; i++)
    {
        var c = line[i];
        
        if (c == '"')
        {
            if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
            {
                currentField += '"';
                i++; // Skip next quote
            }
            else
            {
                inQuotes = !inQuotes;
            }
        }
        else if (c == ',' && !inQuotes)
        {
            fields.Add(currentField);
            currentField = "";
        }
        else
        {
            currentField += c;
        }
    }
    
    fields.Add(currentField);
    return fields.ToArray();
}