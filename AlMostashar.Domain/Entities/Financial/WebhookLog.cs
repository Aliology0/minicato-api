namespace AlMostashar.Domain.Entities;

/// <summary>
/// Lightweight audit log for every webhook received from the payment gateway.
/// Stores the full raw payload for debugging and payment traceability.
/// </summary>
public class WebhookLog : BaseEntity
{
    public int? PaymentId { get; set; }
    public int? TransactionId { get; set; }

    /// <summary>
    /// Full raw JSON payload from the gateway — no length restriction.
    /// </summary>
    public string RawPayload { get; set; } = null!;

    public DateTime ReceivedAt { get; set; }

    /// <summary>
    /// Processing outcome: "Succeeded", "Failed", "Ignored", "Error"
    /// </summary>
    public string ProcessingResult { get; set; } = null!;

    public string? ErrorMessage { get; set; }

    // Navigation
    public Payment? Payment { get; set; }
}
