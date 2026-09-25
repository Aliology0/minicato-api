using AlMostashar.Domain.ValueObject.Enum;

namespace AlMostashar.Application.Features.Reports.Queries.GetReports;

public class ReportListItemDto
{
    public int Id { get; set; }
    public ReportReason Reason { get; set; }
    public ReportStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }

    public int ReporterId { get; set; }
    public string ReporterName { get; set; } = default!;

    public int? ReportedUserId { get; set; }
    public string? ReportedUserName { get; set; }

    public int? CaseId { get; set; }
}
