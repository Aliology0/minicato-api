namespace AlMostashar.Domain.Entities;

public class CaseDocuments : BaseEntity
{
    public string DocumentName { get; set; }
    public string DocumentUrl { get; set; }
    public string DocumentType { get; set; }
    public long SizeInBytes { get; set; }

    public DateTime CreatedAt { get; set; }
    // Pre-request upload flows must set this owner before the document can be linked by ID.
    public int? UploadedByUserId { get; set; }
    public string? CleanupClaimToken { get; set; }
    public DateTime? CleanupClaimedAt { get; set; }

    // FK — nullable: document may belong to a Case, a Request, or both
    public int? CaseId { get; set; }
    public Case? Case { get; set; }

    public int? ClientRequestId { get; set; }
    public ClientRequest? ClientRequest { get; set; }

    public int? ReportId { get; set; }
    public Report? Report { get; set; }

    // 1:1 — CaseDocuments ↔ ChatMessage (optional in case it wasn't from a chat)
    public ChatMessage? ChatMessage { get; set; }
}

