using AlMostashar.Domain.ValueObject;
using AlMostashar.Domain.ValueObject.Enum;

namespace AlMostashar.Domain.Entities;

public class Dispute : BaseEntity
{
    public int CaseId { get; set; }
    public Case Case { get; set; } = null!;

    public int? EscrowId { get; set; }
    public Escrow? Escrow { get; set; }

    public int OpenedByUserId { get; set; }
    public User OpenedByUser { get; set; } = null!;

    public string Reason { get; set; } = string.Empty;
    public List<DisputeAttachment> Attachments { get; set; } = new();
    public List<DisputeChatMessage> DisputeChatMessages { get; set; } = new();

    public DisputeStatus Status { get; set; } = DisputeStatus.Open;
    public DisputePriority Priority { get; set; } = DisputePriority.Medium;

    public string? AdminDecision { get; set; }
    public string? AdminNotes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }

    public int? ReviewedByAdminId { get; set; }
    public Admin? ReviewedByAdmin { get; set; }
}
