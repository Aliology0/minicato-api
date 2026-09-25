using AlMostashar.Application.Features.ClientHome.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.ClientHome.Queries.GetRecentCaseUpdates;

public class GetRecentCaseUpdatesQuery : IRequest<Result<List<RecentCaseUpdateDto>>>
{
    public int Limit { get; set; } = 5;
}
