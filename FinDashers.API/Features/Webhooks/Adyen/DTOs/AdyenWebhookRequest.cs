using System.Text.Json.Serialization;

namespace FinDashers.API.Features.Webhooks.Adyen.DTOs;

public class AdyenWebhookRequest
{
    [JsonPropertyName("live")]
    public string? Live { get; set; }

    [JsonPropertyName("notificationItems")]
    public List<NotificationItem>? NotificationItems { get; set; }
}

public class NotificationItem
{
    [JsonPropertyName("NotificationRequestItem")]
    public NotificationRequestItem? NotificationRequestItem { get; set; }

    [JsonPropertyName("AdditionalProperties")]
    public Dictionary<string, object>? AdditionalProperties { get; set; }
}

public class NotificationRequestItem
{
    [JsonPropertyName("additionalData")]
    public Dictionary<string, object>? AdditionalData { get; set; }

    [JsonPropertyName("amount")]
    public Amount? Amount { get; set; }

    [JsonPropertyName("eventCode")]
    public string? EventCode { get; set; }

    [JsonPropertyName("eventDate")]
    public string? EventDate { get; set; }

    [JsonPropertyName("merchantAccountCode")]
    public string? MerchantAccountCode { get; set; }

    [JsonPropertyName("merchantReference")]
    public string? MerchantReference { get; set; }

    [JsonPropertyName("paymentMethod")]
    public string? PaymentMethod { get; set; }

    [JsonPropertyName("pspReference")]
    public string? PspReference { get; set; }

    [JsonPropertyName("reason")]
    public string? Reason { get; set; }

    [JsonPropertyName("success")]
    public string? Success { get; set; }
}

public class Amount
{
    [JsonPropertyName("currency")]
    public string? Currency { get; set; }

    [JsonPropertyName("value")]
    public long Value { get; set; }
}
