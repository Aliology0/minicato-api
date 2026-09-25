using AlMostashar.Application.Common.Models;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;

namespace AlMostashar.Application.Features.Reports.Queries.GetReports;

public class GetReportsQuery : IRequest<Result<CursorPagedResult<ReportListItemDto>>>
{
    public ReportStatus? Status { get; set; }

    /// <summary>Id of the last item from the previous page.</summary>
    public int? Cursor { get; set; }

    /// <summary>CreatedAt of the last item from the previous page (used together with Cursor for tie-breaking).</summary>
    public DateTime? CursorDate { get; set; }

    public int PageSize { get; set; } = 20;
}
