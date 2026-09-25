using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;


namespace AlMostashar.Application.Features.Reports.Commands.CreateReport;

public class CreateReportCommand : IRequest<Result<string>>
{
    /// <summary>
    /// The UserId of the lawyer being reported.
    /// Provide either LawyerId OR UserId — at least one is required when reporting a user.
    /// Leave both null when reporting a platform issue.
    /// </summary>
    public int? LawyerId { get; set; }

    /// <summary>
    /// Direct UserId of the reported user (alternative to LawyerId).
    /// </summary>
    public int? UserId { get; set; }

    /// <summary>
    /// Optional CaseId if the report is related to a specific case.
    /// </summary>
    public int? CaseId { get; set; }

    /// <summary>Required. Description of the problem (max 4000 chars).</summary>
    public string ProblemContent { get; set; } = default!;

    /// <summary>
    /// Optional list of chat messages submitted as evidence.
    /// Because chats are end-to-end encrypted, the frontend decrypts them locally
    /// and sends the plaintext messages directly from the device's local database.
    /// </summary>
    public List<ReportMessageEvidence>? Messages { get; set; }

    /// <summary>
    /// IDs returned by POST api/documents/upload.
    /// </summary>
    public List<int>? AttachmentIds { get; set; }

    /// <summary>The reason for the report.</summary>
    public ReportReason Reason { get; set; }
}
