using AlMostashar.Domain.ValueObject.Enum;

namespace AlMostashar.Domain.Entities;

public class WithdrawalRequest : BaseEntity
{
    public int LawyerId { get; set; }
    public Lawyer Lawyer { get; set; }

    public int WalletId { get; set; }
    public Wallet Wallet { get; set; }

    public decimal Amount { get; set; }
    public WithdrawalMethod Method { get; set; }
    public string AccountDetailsEncrypted { get; set; } = string.Empty;
    public string AccountDetailsMasked { get; set; } = string.Empty;
    public WithdrawalStatus Status { get; set; } = WithdrawalStatus.Pending;
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }
    public int? ReviewedByAdminId { get; set; }
    public string? AdminNotes { get; set; }
    public string? RejectionReason { get; set; }
    public string? PayoutReference { get; set; }
    public string? PayoutProvider { get; set; }
    public int? PaidByAdminId { get; set; }
    public DateTime? PaidAt { get; set; }
    public int? WalletTransactionId { get; set; }

    public ICollection<WalletTransaction> WalletTransactions { get; set; } = new List<WalletTransaction>();

    public bool CanTransitionTo(WithdrawalStatus nextStatus)
    {
        return IsValidTransition(Status, nextStatus);
    }

    public static bool IsValidTransition(WithdrawalStatus currentStatus, WithdrawalStatus nextStatus)
    {
        return currentStatus switch
        {
            WithdrawalStatus.Pending => nextStatus is WithdrawalStatus.Approved
                or WithdrawalStatus.Cancelled
                or WithdrawalStatus.Rejected,
            WithdrawalStatus.Approved => nextStatus is WithdrawalStatus.Paid
                or WithdrawalStatus.Rejected,
            _ => false
        };
    }

    public static bool IsTerminal(WithdrawalStatus status)
    {
        return status is WithdrawalStatus.Rejected
            or WithdrawalStatus.Cancelled
            or WithdrawalStatus.Paid;
    }
}
