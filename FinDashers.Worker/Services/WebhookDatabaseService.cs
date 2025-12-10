using Dapper;
using FinDashers.API.Features.Webhooks.Adyen.Models;
using Npgsql;

namespace FinDashers.Worker.Services;

public interface IWebhookDatabaseService
{
    Task<bool> InsertAdyenTransactionAsync(AdyenTransaction transaction);
}

public class WebhookDatabaseService : IWebhookDatabaseService
{
    private readonly string _connectionString;
    private readonly ILogger<WebhookDatabaseService> _logger;

    public WebhookDatabaseService(IConfiguration configuration, ILogger<WebhookDatabaseService> logger)
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
}
