using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Common.Models;
using AlMostashar.Application.Features.Disputes.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.Disputes.Queries.GetDisputesList;

public class GetDisputesListQueryHandler : IRequestHandler<GetDisputesListQuery, Result<CursorPagedResult<DisputeListItemDto>>>
{
    private readonly IAppDbContext _context;

    public GetDisputesListQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<CursorPagedResult<DisputeListItemDto>>> Handle(GetDisputesListQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Disputes
            .Include(d => d.OpenedByUser)
            .AsNoTracking();

        if (request.Status.HasValue)
        {
            query = query.Where(d => d.Status == request.Status.Value);
        }

        if (request.CursorDate.HasValue && request.Cursor.HasValue)
        {
            var cursorDate = request.CursorDate.Value;
            var cursorId = request.Cursor.Value;
            
            query = query.Where(d => d.CreatedAt < cursorDate || (d.CreatedAt == cursorDate && d.Id < cursorId));
        }

        var pageSize = Math.Clamp(request.PageSize, 1, 50);

        var disputes = await query
            .OrderByDescending(d => d.CreatedAt)
            .ThenByDescending(d => d.Id)
            .Take(pageSize + 1)
            .Select(d => new DisputeListItemDto
            {
                Id = d.Id,
                CaseId = d.CaseId,
                EscrowId = d.EscrowId,
                OpenedByUserId = d.OpenedByUserId,
                OpenedByUserName = d.OpenedByUser.FullName,
                Reason = d.Reason,
                Status = d.Status,
                Priority = d.Priority,
                CreatedAt = d.CreatedAt
            })
            .ToListAsync(cancellationToken);

        bool hasNextPage = disputes.Count > pageSize;
        if (hasNextPage)
        {
            disputes.RemoveAt(pageSize);
        }

        int? nextCursor = hasNextPage ? disputes.Last().Id : null;
        DateTime? nextCursorDate = hasNextPage ? disputes.Last().CreatedAt : null;

        return Result<CursorPagedResult<DisputeListItemDto>>.Success(
            new CursorPagedResult<DisputeListItemDto>
            {
                Items = disputes,
                NextCursor = nextCursor,
                NextCursorDate = nextCursorDate,
                HasMore = hasNextPage
            }
        );
    }
}
