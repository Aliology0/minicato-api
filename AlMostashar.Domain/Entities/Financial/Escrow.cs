using AlMostashar.Domain.ValueObject.Enum;
namespace AlMostashar.Domain.Entities;
public class Escrow: BaseEntity
{
    public ClientRequest Request { get; set; }
    public int RequestId { get; set; }
    public Decimal Amount { get; set; }
    public EscrowStatus Status { get; set; }

    // ─── Payment audit relationship ───
    // Nullable: legacy/NotFunded escrows may not have a payment yet.
    public int? PaymentId { get; set; }
    public Payment? Payment { get; set; }

    public DateTime? EscrowFundedAt { get; set; }

    public DateTime? EscrowReleasedAt { get; set; }

    public DateTime? EscrowRefundedAt { get; set; }

    public DateTime? EscrowDisputedAt { get; set; }

    // ─── Navigation: 1:N (Escrow → WalletTransaction) ───
    public ICollection<WalletTransaction> WalletTransactions { get; set; } = new List<WalletTransaction>();
}