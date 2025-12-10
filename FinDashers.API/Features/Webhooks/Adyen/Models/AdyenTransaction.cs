namespace FinDashers.API.Features.Webhooks.Adyen.Models;

public class AdyenTransaction
{
    public long Id { get; set; }
    public string PspReference { get; set; } = string.Empty;
    public string? MerchantReference { get; set; }
    public string EventCode { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public decimal ApprovedAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string? MerchantAccount { get; set; }
    public string? PaymentMethod { get; set; }
    public string? Reason { get; set; }
    public bool Success { get; set; }
    public string? LocationId { get; set; }
    public int? CompanyId { get; set; }
    public string? TerminalId { get; set; }
    public string? TenderReference { get; set; }
    public string RawEvent { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
