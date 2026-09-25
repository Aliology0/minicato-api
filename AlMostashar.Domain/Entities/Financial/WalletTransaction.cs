using AlMostashar.Domain.ValueObject.Enum;

namespace AlMostashar.Domain.Entities;

public class WalletTransaction : BaseEntity
{
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public string ReferenceType { get; set; } = string.Empty;
    public string ReferenceId { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? BalanceAfter { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // FK — Step 4: 1:N (Wallet → WalletTransaction)
    public int WalletId { get; set; }
    public Wallet Wallet { get; set; }

    // FK — optional many-to-one (Escrow → WalletTransaction)
    public int? EscrowId { get; set; }
    public Escrow? Escrow { get; set; }

    public int? CaseId { get; set; }

    public int? InvoiceId { get; set; }

    public int? WithdrawalRequestId { get; set; }
    public WithdrawalRequest? WithdrawalRequest { get; set; }

    public PaymentWalletTransaction? PaymentWalletTransaction { get; set; }
}
