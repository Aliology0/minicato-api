namespace AlMostashar.Domain.Entities;

public class PaymentWalletTransaction
{

    // The ID from the payment table
    public int PaymentId { get; set; }

    // The ID from the wallet transaction table
    public int WalletTransactionId { get; set; }

    // Date and time when we added this link
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Connect this class to the Payment class
    public virtual Payment Payment { get; set; }

    // Connect this class to the WalletTransaction class
    public virtual WalletTransaction WalletTransaction { get; set; }
}

