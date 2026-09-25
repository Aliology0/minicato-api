using AlMostashar.Domain.ValueObject.Enum;

namespace AlMostashar.Domain.Entities;

public class Report : BaseEntity
{
    public ReportReason Reason { get; set; }
    public ReportStatus Status { get; set; }
    public string Description { get; set; }
    public string? AdminNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }

    /// <summary>
    /// Optional JSON-serialized list of decrypted chat messages submitted as evidence.
    /// Messages are decrypted on the user's device and sent as plaintext since the server
    /// cannot decrypt end-to-end encrypted chats.
    /// </summary>
    public string? AttachedMessages { get; set; }

    // Legacy storage-key evidence. New writes use Attachments by document ID.
    [Obsolete("Use Attachments. Storage keys must not cross the API boundary.")]
    public string? AttachmentUrls { get; set; }

    public ICollection<CaseDocuments> Attachments { get; set; } = new List<CaseDocuments>();

    // FK — N:1 (Reporter — the user who filed the report)
    public int ReporterId { get; set; }
    public User Reporter { get; set; }

    // FK — N:1 (ReportedUser — nullable when reporting the platform itself)
    public int? ReportedUserId { get; set; }
    public User? ReportedUser { get; set; }

    // FK — N:1 (Case — nullable; report may not be tied to a case)
    public int? CaseId { get; set; }
    public Case? Case { get; set; }

    // FK — N:1 (ReviewedByAdmin — the admin who reviewed this report)
    public int? ReviewedByAdminId { get; set; }
    public Admin? ReviewedByAdmin { get; set; }
}
