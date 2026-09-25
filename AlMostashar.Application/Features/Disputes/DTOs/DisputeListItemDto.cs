using AlMostashar.Domain.ValueObject.Enum;

namespace AlMostashar.Application.Features.Disputes.DTOs;

public class DisputeListItemDto
{
    public int Id { get; set; }
    public int CaseId { get; set; }
    public int? EscrowId { get; set; }
    public int OpenedByUserId { get; set; }
    public string OpenedByUserName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DisputeStatus Status { get; set; }
    public DisputePriority Priority { get; set; }
    public DateTime CreatedAt { get; set; }
}
