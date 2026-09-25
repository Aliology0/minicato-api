using AlMostashar.Domain.ValueObject.Enum;

namespace AlMostashar.Application.Features.Escrows.DTOs;

public class EscrowOperationResponseDto
{
    public string Message { get; set; } = string.Empty;
    public int EscrowId { get; set; }
    public int RequestId { get; set; }
    public decimal Amount { get; set; }
    public decimal? ReleasedAmount { get; set; }
    public EscrowStatus Status { get; set; }
}
