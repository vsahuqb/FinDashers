using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using FinDashers.API.Features.Webhooks.Adyen.Attributes;
using FinDashers.API.Features.Webhooks.Adyen.DTOs;
using FinDashers.API.Features.Webhooks.Adyen.Models;
using FinDashers.API.Features.Webhooks.Adyen.Services;

namespace FinDashers.API.Features.Webhooks.Adyen.Controllers;

[ApiController]
[Route("api/[controller]")]
[AdyenBasicAuthentication]
public class AdyenWebhookController : ControllerBase
{
    private readonly IAdyenHmacValidationService _hmacValidationService;
    private readonly IAdyenDatabaseService _databaseService;
    private readonly ILogger<AdyenWebhookController> _logger;

    public AdyenWebhookController(
        IAdyenHmacValidationService hmacValidationService,
        IAdyenDatabaseService databaseService,
        ILogger<AdyenWebhookController> logger)
    {
        _hmacValidationService = hmacValidationService;
        _databaseService = databaseService;
        _logger = logger;
    }

    /// <summary>
    /// Receives and processes Adyen webhook notifications.
    /// Validates HMAC signature and stores transaction data in PostgreSQL.
    /// </summary>
    [HttpPost("notifications")]
    public async Task<IActionResult> ReceiveNotification()
    {
        try
        {
            // Read the raw request body
            string rawBody;
            using (var reader = new StreamReader(Request.Body))
            {
                rawBody = await reader.ReadToEndAsync();
            }

            _logger.LogInformation($"Received Adyen webhook: {rawBody}");

            // Deserialize the webhook request
            var webhookRequest = JsonSerializer.Deserialize<AdyenWebhookRequest>(rawBody);

            if (webhookRequest?.NotificationItems == null || webhookRequest.NotificationItems.Count == 0)
            {
                _logger.LogWarning("No notification items found in webhook request");
                return Ok(new { notificationResponse = "[accepted]" });
            }

            // Process each notification item
            foreach (var notificationItem in webhookRequest.NotificationItems)
            {
                var requestItem = notificationItem.NotificationRequestItem;
                if (requestItem == null)
                {
                    _logger.LogWarning("NotificationRequestItem is null");
                    continue;
                }

                // Extract HMAC signature from additionalData
                string? hmacSignature = null;
                if (requestItem.AdditionalData?.TryGetValue("hmacSignature", out var hmacObj) == true)
                {
                    hmacSignature = hmacObj?.ToString();
                }

                if (string.IsNullOrWhiteSpace(hmacSignature))
                {
                    _logger.LogWarning("HMAC signature not found in additionalData");
                    return BadRequest(new { error = "HMAC signature missing" });
                }

                // Extract merchant account code for credential lookup
                string? merchantAccountCode = requestItem.MerchantAccountCode;
                if (string.IsNullOrWhiteSpace(merchantAccountCode))
                {
                    _logger.LogWarning("Merchant account code not found");
                    return BadRequest(new { error = "Merchant account code missing" });
                }

                // Extract metadata from additionalData
                var metadata = ExtractMetadata(requestItem.AdditionalData);

                // Create transaction model
                var transaction = new AdyenTransaction
                {
                    PspReference = requestItem.PspReference ?? string.Empty,
                    MerchantReference = requestItem.MerchantReference,
                    EventCode = requestItem.EventCode ?? string.Empty,
                    EventDate = ParseEventDate(requestItem.EventDate),
                    ApprovedAmount = requestItem.Amount?.Value / 100m ?? 0,
                    Currency = requestItem.Amount?.Currency ?? string.Empty,
                    MerchantAccount = requestItem.MerchantAccountCode,
                    PaymentMethod = requestItem.PaymentMethod,
                    Reason = requestItem.Reason,
                    Success = requestItem.Success?.ToLower() == "true",
                    LocationId = metadata.LocationId,
                    CompanyId = metadata.CompanyId,
                    TerminalId = metadata.TerminalId ?? requestItem.AdditionalData?["terminalId"]?.ToString(),
                    TenderReference = requestItem.AdditionalData?["tenderReference"]?.ToString(),
                    RawEvent = JsonSerializer.Serialize(requestItem),
                    CreatedAt = DateTime.UtcNow
                };

                // Insert into database
                await _databaseService.InsertAdyenTransactionAsync(transaction);

                _logger.LogInformation($"Successfully processed Adyen notification for PSP Reference: {transaction.PspReference}");
            }

            // Return Adyen's expected response
            return Ok(new { notificationResponse = "[accepted]" });
        }
        catch (JsonException ex)
        {
            _logger.LogError($"JSON deserialization error: {ex.Message}");
            return BadRequest(new { error = "Invalid JSON format" });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unexpected error processing webhook: {ex.Message}");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Extracts metadata from the additionalData field.
    /// </summary>
    private MetadataInfo ExtractMetadata(Dictionary<string, object>? additionalData)
    {
        var metadata = new MetadataInfo();

        if (additionalData == null)
            return metadata;

        // Try to extract metadata object
        if (additionalData.TryGetValue("metadata", out var metadataObj) && metadataObj is JsonElement metadataElement)
        {
            try
            {
                var metadataDict = JsonSerializer.Deserialize<Dictionary<string, object>>(metadataElement.GetRawText());
                if (metadataDict != null)
                {
                    if (metadataDict.TryGetValue("LocationId", out var locationId))
                        metadata.LocationId = locationId?.ToString();

                    if (metadataDict.TryGetValue("CompanyId", out var companyId) && int.TryParse(companyId?.ToString(), out var companyIdInt))
                        metadata.CompanyId = companyIdInt;

                    if (metadataDict.TryGetValue("TerminalId", out var terminalId))
                        metadata.TerminalId = terminalId?.ToString();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error extracting metadata: {ex.Message}");
            }
        }

        return metadata;
    }

    /// <summary>
    /// Parses the event date string to DateTime.
    /// </summary>
    private DateTime ParseEventDate(string? eventDateStr)
    {
        if (string.IsNullOrWhiteSpace(eventDateStr))
            return DateTime.UtcNow;

        if (DateTime.TryParse(eventDateStr, out var parsedDate))
            return parsedDate;

        _logger.LogWarning($"Could not parse event date: {eventDateStr}");
        return DateTime.UtcNow;
    }

    /// <summary>
    /// Helper class to hold extracted metadata.
    /// </summary>
    private class MetadataInfo
    {
        public string? LocationId { get; set; }
        public int? CompanyId { get; set; }
        public string? TerminalId { get; set; }
    }
}
