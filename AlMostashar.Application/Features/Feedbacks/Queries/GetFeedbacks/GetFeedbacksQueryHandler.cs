using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Common.Models;
using AlMostashar.Application.Features.Feedbacks.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.Feedbacks.Queries.GetFeedbacks;

public class GetFeedbacksQueryHandler : IRequestHandler<GetFeedbacksQuery, Result<CursorPagedResult<FeedbackDto>>>
{
    private readonly IAppDbContext _context;

    public GetFeedbacksQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<CursorPagedResult<FeedbackDto>>> Handle(GetFeedbacksQuery request, CancellationToken cancellationToken)
    {
        int? targetLawyerId = request.LawyerId;

        // Force LawyerId if the caller is a Lawyer
        if (request.IsLawyerRole)
        {
            targetLawyerId = request.CurrentUserId;
        }

        var query = _context.Feedbacks
            .Include(f => f.ClientRequest)
                .ThenInclude(cr => cr.Client)
            .AsNoTracking()
            .AsQueryable();

        if (targetLawyerId.HasValue)
        {
            query = query.Where(f => f.LawyerServiceLawyerId == targetLawyerId.Value);
        }

        if (request.ServiceId.HasValue)
        {
            query = query.Where(f => f.LawyerServiceLegalServiceId == request.ServiceId.Value);
        }

        if (request.NextCursor.HasValue)
        {
            query = query.Where(f => f.Id < request.NextCursor.Value);
        }

        query = query.OrderByDescending(f => f.Id).Take(request.PageSize + 1);

        var feedbacks = await query.Select(f => new FeedbackDto
        {
            Id = f.Id,
            ClientProfilePhoto = f.ClientRequest.Client.AvatarUrl ?? string.Empty,
            FirstName = f.ClientRequest.Client.FirstName,
            LastName = f.ClientRequest.Client.LastName,
            Rate = f.Rate,
            CreateAt = f.ClientRequest.CreatedAt,
            FeedbackContent = f.Content ?? string.Empty
        }).ToListAsync(cancellationToken);

        bool hasMore = feedbacks.Count > request.PageSize;
        int? nextCursor = null;

        if (hasMore)
        {
            feedbacks.RemoveAt(feedbacks.Count - 1);
            nextCursor = feedbacks[^1].Id;
        }

        var pagedResult = new CursorPagedResult<FeedbackDto>
        {
            HasMore = hasMore,
            NextCursor = nextCursor,
            Items = feedbacks
        };

        return Result<CursorPagedResult<FeedbackDto>>.Success(pagedResult);
    }
}
