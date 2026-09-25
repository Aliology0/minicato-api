using AlMostashar.Application.Features.Reports.Commands.CreateReport;
using AlMostashar.Application.Features.ClientRequests.DTOs;
using AlMostashar.Domain.ValueObject.Enum;

namespace AlMostashar.Application.Features.Reports.Queries.GetReportDetails;

public class ReportDetailsDto
{
    public int Id { get; set; }
    public ReportReason Reason { get; set; }
    public string Description { get; set; } = default!;
    public ReportStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? AdminNotes { get; set; }

    public int ReporterId { get; set; }
    public string ReporterName { get; set; } = default!;
    public UserRole ReporterRole { get; set; }

    public int? ReportedUserId { get; set; }
    public string? ReportedUserName { get; set; }
    public UserRole? ReportedUserRole { get; set; }

    public int? CaseId { get; set; }

    // Decoded evidence lists
    public List<ReportMessageEvidence> Messages { get; set; } = new();
    public List<DocumentDto> Documents { get; set; } = new();
}
