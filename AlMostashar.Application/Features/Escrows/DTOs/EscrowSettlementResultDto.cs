namespace AlMostashar.Application.Features.Escrows.DTOs;

public class EscrowSettlementResultDto
{
    public int EscrowId { get; set; }
    public int RequestId { get; set; }
    public int LawyerId { get; set; }
    public int WalletId { get; set; }
    public int WalletTransactionId { get; set; }
    public int? PaymentId { get; set; }
    public int? InvoiceId { get; set; }
    public int? CaseId { get; set; }
    public decimal EscrowAmount { get; set; }
    public decimal ReleasedAmount { get; set; }
}
