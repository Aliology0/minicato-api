namespace AlMostashar.Application.Features.Wallets.DTOs;

public class WalletSummaryDto
{
    public decimal AvailableBalance { get; set; }
    public decimal TotalEarnings { get; set; }
    public decimal EscrowBalance { get; set; }
    public decimal PendingWithdrawalBalance { get; set; }
    public decimal PaidWithdrawalBalance { get; set; }
    public decimal TotalWithdrawn { get; set; }

    // Backward-compatible field for existing clients.
    // In the wallet MVP it represents funds reserved for pending/approved withdrawals.
    public decimal PendingBalance { get; set; }
}
