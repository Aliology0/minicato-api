using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Common.Models;
using AlMostashar.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.Reports.Queries.GetReports;

public class GetReportsQueryHandler : IRequestHandler<GetReportsQuery, Result<CursorPagedResult<ReportListItemDto>>>
{
    private readonly IAppDbContext _context;

    public GetReportsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<CursorPagedResult<ReportListItemDto>>> Handle(
        GetReportsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Reports
            .Include(r => r.Reporter)
            .Include(r => r.ReportedUser)
            .AsNoTracking();

        if (request.Status.HasValue)
        {
            query = query.Where(r => r.Status == request.Status.Value);
        }

        // Apply cursor pagination logic (sorting by CreatedAt DESC, Id DESC)
        if (request.CursorDate.HasValue && request.Cursor.HasValue)
        {
            var cursorDate = request.CursorDate.Value;
            var cursorId = request.Cursor.Value;
            
            query = query.Where(r => r.CreatedAt < cursorDate || (r.CreatedAt == cursorDate && r.Id < cursorId));
        }

        var pageSize = Math.Clamp(request.PageSize, 1, 50);

        var reports = await query
            .OrderByDescending(r => r.CreatedAt)
            .ThenByDescending(r => r.Id)
            .Take(pageSize + 1) // Take one extra to determine if there's a next page
            .Select(r => new ReportListItemDto
            {
                Id = r.Id,
                Reason = r.Reason,
                Status = r.Status,
                CreatedAt = r.CreatedAt,
                ReporterId = r.ReporterId,
                ReporterName = r.Reporter.FullName,
                ReportedUserId = r.ReportedUserId,
                ReportedUserName = r.ReportedUser != null ? r.ReportedUser.FullName : null,
                CaseId = r.CaseId
            })
            .ToListAsync(cancellationToken);

        bool hasNextPage = reports.Count > pageSize;
        if (hasNextPage)
        {
            reports.RemoveAt(pageSize);
        }

        int? nextCursor = hasNextPage ? reports.Last().Id : null;
        DateTime? nextCursorDate = hasNextPage ? reports.Last().CreatedAt : null;

        return Result<CursorPagedResult<ReportListItemDto>>.Success(
            new CursorPagedResult<ReportListItemDto>
            {
                Items = reports,
                NextCursor = nextCursor,
                NextCursorDate = nextCursorDate,
                HasMore = hasNextPage
            }
        );
    }
}
