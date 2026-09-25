namespace AlMostashar.Domain.Entities;

public class Wallet : BaseEntity
{
    public decimal AvailableBalance { get; set; }
    public decimal Total { get; set; }

    // FK — Step 3: 1:1 (Lawyer → Wallet)
    public int LawyerId { get; set; }
    public Lawyer Lawyer { get; set; }

    // Navigation — Step 4: 1:N (Wallet → WalletTransaction)
    public ICollection<WalletTransaction> WalletTransactions { get; set; } = new List<WalletTransaction>();

    public ICollection<WithdrawalRequest> WithdrawalRequests { get; set; } = new List<WithdrawalRequest>();

}
