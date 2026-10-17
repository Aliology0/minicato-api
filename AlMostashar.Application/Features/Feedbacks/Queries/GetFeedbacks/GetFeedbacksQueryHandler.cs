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
    private readonly AlMostashar.Application.Common.Interfaces.IStorageService _storageService;

    public GetFeedbacksQueryHandler(IAppDbContext context, AlMostashar.Application.Common.Interfaces.IStorageService storageService)
    {
        _context = context;
        _storageService = storageService;
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

        var raw = await query.Select(f => new
        {
            f.Id,
            Avatar = f.ClientRequest.Client.AvatarUrl,
            FirstName = f.ClientRequest.Client.FirstName,
            LastName = f.ClientRequest.Client.LastName,
            f.Rate,
            CreateAt = f.ClientRequest.CreatedAt,
            FeedbackContent = f.Content
        }).ToListAsync(cancellationToken);

        var feedbacks = raw.Select(f => new FeedbackDto
        {
            Id = f.Id,
            ClientProfilePhoto = AlMostashar.Application.Common.Helpers.StorageUrlResolver.ResolvePublicUrl(_storageService, f.Avatar) ?? string.Empty,
            FirstName = f.FirstName,
            LastName = f.LastName,
            Rate = f.Rate,
            CreateAt = f.CreateAt,
            FeedbackContent = f.FeedbackContent ?? string.Empty
        }).ToList();

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