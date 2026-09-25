using AlMostashar.Domain.ValueObject.Enum;

namespace AlMostashar.Application.Features.Wallets.DTOs;

public class WalletTransactionDto
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public string? ReferenceType { get; set; }
    public string? ReferenceId { get; set; }
    public int? EscrowId { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public decimal? BalanceAfter { get; set; }
    public int? CaseId { get; set; }
    public int? InvoiceId { get; set; }
    public int? WithdrawalRequestId { get; set; }
}
