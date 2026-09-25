using AlMostashar.Domain.ValueObject.Enum;

namespace AlMostashar.Application.Features.Wallets.DTOs;

public class WithdrawalRequestDto
{
    public int Id { get; set; }
    public int LawyerId { get; set; }
    public int WalletId { get; set; }
    public decimal Amount { get; set; }
    public WithdrawalMethod Method { get; set; }
    // Backward-compatible display field. List endpoints return the masked value here.
    public string AccountDetails { get; set; } = string.Empty;
    public string AccountDetailsMasked { get; set; } = string.Empty;
    public string? AccountDetailsFull { get; set; }
    public WithdrawalStatus Status { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public int? ReviewedByAdminId { get; set; }
    public string? AdminNotes { get; set; }
    public string? RejectionReason { get; set; }
    public string? PayoutReference { get; set; }
    public string? PayoutProvider { get; set; }
    public int? PaidByAdminId { get; set; }
    public DateTime? PaidAt { get; set; }
    public int? WalletTransactionId { get; set; }
    public string? LawyerName { get; set; }
    public string? LawyerEmail { get; set; }
}
