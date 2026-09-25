using AlMostashar.Domain.ValueObject.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlMostashar.Domain.Entities;

public class Payment : BaseEntity
{
    // --- Core Payment Info ---
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "EGP"; // Default currency
    public DateTime PayDate { get; set; }
    public PaymentStatus Status { get; set; }

    // Set by webhook — unknown at creation time
    public string? PaymentMethod { get; set; }

    // --- Accounting & Escrow Fields ---
    // The fee Paymob took for this transaction
    public decimal? ProviderFee { get; set; }
    // The actual money that reached your bank account (Amount - ProviderFee)
    public decimal? NetAmount { get; set; }
    // Track if any part of this payment was refunded to the client
    public decimal RefundedAmount { get; set; } = 0;

    // --- Provider (Gateway) Integration ---
    public PaymentProvider PaymentProvider { get; set; }

    // The unique transaction ID from Paymob — set by webhook (nullable until webhook arrives)
    public int? TransactionId { get; set; }

    // The Paymob intention ID — secondary/debugging identifier, set after intention creation
    public string? IntentionId { get; set; }

    // The order ID created inside Paymob's system (e.g., 217503754)
    public string? ProviderOrderId { get; set; }

    // Summarized gateway response snapshot — full raw payload stored in WebhookLog
    public string? GatewayResponse { get; set; }

    // --- Relationships ---

    // FK — 1:1 (Invoice → Payment)
    public int InvoiceId { get; set; }
    public Invoice Invoice { get; set; }

    // Navigation to the bridging entity linking a payment to a wallet transaction
    // (Typically used during settlement/release phases, not upon initial payment success)
    public PaymentWalletTransaction PaymentWalletTransaction { get; set; }

    // Navigation — optional 1:1 (Payment → Escrow)
    public Escrow? Escrow { get; set; }
}

