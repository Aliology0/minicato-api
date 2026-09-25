using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.ClientHome.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.ClientHome.Queries.GetRecentCaseUpdates;

public class GetRecentCaseUpdatesQueryHandler : IRequestHandler<GetRecentCaseUpdatesQuery, Result<List<RecentCaseUpdateDto>>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetRecentCaseUpdatesQueryHandler(IAppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<List<RecentCaseUpdateDto>>> Handle(GetRecentCaseUpdatesQuery request, CancellationToken cancellationToken)
    {
        var clientId = _currentUser.UserId;

        var recentUpdates = await _context.CaseTimelines
            .Include(t => t.Case)
                .ThenInclude(c => c.CaseClientRequest)
                    .ThenInclude(ccr => ccr.ClientRequest)
            .Where(t => t.Case.CaseClientRequest != null 
                        && t.Case.CaseClientRequest.ClientRequest != null 
                        && t.Case.CaseClientRequest.ClientRequest.ClientId == clientId)
            .OrderByDescending(t => t.CreatedAt)
            .Take(request.Limit)
            .Select(t => new RecentCaseUpdateDto
            {
                CaseId = t.CaseId,
                CaseTitle = t.Case.Title,
                UpdateType = t.Title,
                Message = t.Content,
                CreatedAt = t.CreatedAt,
                Status = t.Case.Status.ToString()
            })
            .ToListAsync(cancellationToken);

        return Result<List<RecentCaseUpdateDto>>.Success(recentUpdates);
    }
}
