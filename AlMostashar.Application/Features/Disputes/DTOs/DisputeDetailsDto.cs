using AlMostashar.Domain.ValueObject.Enum;

namespace AlMostashar.Application.Features.Disputes.DTOs;

public class DisputeDetailsDto
{
    public int Id { get; set; }
    public int CaseId { get; set; }
    public int? EscrowId { get; set; }
    public int OpenedByUserId { get; set; }
    public string OpenedByUserName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DisputeStatus Status { get; set; }
    public DisputePriority Priority { get; set; }
    public string? AdminDecision { get; set; }
    public string? AdminNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public int? ReviewedByAdminId { get; set; }
    public string? ReviewedByAdminName { get; set; }

    public DateTime? LastActivityAt { get; set; }
    public DisputePartyDto? Initiator { get; set; }
    public DisputePartyDto? Respondent { get; set; }
    public DisputeFinancialsDto? Financials { get; set; }
    public List<DisputeAttachmentDto> Attachments { get; set; } = new();
    public List<DisputeChatMessageDto> DisputeChatMessages { get; set; } = new();
}

public class DisputePartyDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Avatar { get; set; }
    public string Role { get; set; } = string.Empty; // "العميل" or "المستشار"
}

public class DisputeFinancialsDto
{
    public int? PaymentId { get; set; }
    public decimal Amount { get; set; }
    public decimal EscrowAmount { get; set; }
    public string Currency { get; set; } = "EGP";
    public string? PaymentMethod { get; set; }
}

public class DisputeAttachmentDto
{
    public string Name { get; set; } = string.Empty;
    public long Size { get; set; }
    public string Path { get; set; } = string.Empty;
}

public class DisputeChatMessageDto
{
    public int SenderId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
}

