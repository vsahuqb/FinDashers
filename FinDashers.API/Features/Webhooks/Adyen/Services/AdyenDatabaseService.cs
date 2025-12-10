using Dapper;
using FinDashers.API.Features.Webhooks.Adyen.Models;
using Npgsql;

namespace FinDashers.API.Features.Webhooks.Adyen.Services;

public interface IAdyenDatabaseService
{
    Task<bool> InsertAdyenTransactionAsync(AdyenTransaction transaction);
    Task<string?> GetWebhookCredentialAsync(string username);
}

public class AdyenDatabaseService : IAdyenDatabaseService
{
    private readonly string _connectionString;
    private readonly ILogger<AdyenDatabaseService> _logger;

    public AdyenDatabaseService(IConfiguration configuration, ILogger<AdyenDatabaseService> logger)
    {
        _connectionString = configuration.GetConnectionString("PostgreSQL") 
            ?? throw new InvalidOperationException("PostgreSQL connection string not found");
        _logger = logger;
    }

    public async Task<bool> InsertAdyenTransactionAsync(AdyenTransaction transaction)
    {
        try
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                const string query = @"
                    INSERT INTO adyen_transactions (
                        psp_reference, merchant_reference, event_code, event_date,
                        approved_amount, currency, merchant_account, payment_method,
                        reason, success, location_id, company_id, terminal_id,
                        tender_reference, raw_event, created_at
                    ) VALUES (
                        @PspReference, @MerchantReference, @EventCode, @EventDate,
                        @ApprovedAmount, @Currency, @MerchantAccount, @PaymentMethod,
                        @Reason, @Success, @LocationId, @CompanyId, @TerminalId,
                        @TenderReference, @RawEvent::jsonb, @CreatedAt
                    )";

                var result = await connection.ExecuteAsync(query, new
                {
                    transaction.PspReference,
                    transaction.MerchantReference,
                    transaction.EventCode,
                    transaction.EventDate,
                    transaction.ApprovedAmount,
                    transaction.Currency,
                    transaction.MerchantAccount,
                    transaction.PaymentMethod,
                    transaction.Reason,
                    transaction.Success,
                    transaction.LocationId,
                    transaction.CompanyId,
                    transaction.TerminalId,
                    transaction.TenderReference,
                    transaction.RawEvent,
                    transaction.CreatedAt
                });

                _logger.LogInformation($"Successfully inserted Adyen transaction with PSP Reference: {transaction.PspReference}");
                return result > 0;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error inserting Adyen transaction: {ex.Message}");
            throw;
        }
    }

    public async Task<string?> GetWebhookCredentialAsync(string username)
    {
        try
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                const string query = @"
                    SELECT password FROM webhook_credentials 
                    WHERE username = @Username AND is_active = true";

                var password = await connection.QueryFirstOrDefaultAsync<string>(query, new { Username = username });
                return password;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving webhook credential: {ex.Message}");
            throw;
        }
    }
}
