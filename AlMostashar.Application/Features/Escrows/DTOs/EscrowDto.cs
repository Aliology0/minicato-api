using AlMostashar.Domain.ValueObject.Enum;

namespace AlMostashar.Application.Features.Escrows.DTOs;

public class EscrowDto
{
    public int EscrowId { get; set; }
    public int RequestId { get; set; }
    public decimal Amount { get; set; }
    public EscrowStatus Status { get; set; }
    public DateTime? EscrowFundedAt { get; set; }
    public DateTime? EscrowReleasedAt { get; set; }
    public DateTime? EscrowRefundedAt { get; set; }
}
