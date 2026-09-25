using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Common.Models;
using AlMostashar.Application.Features.Cases.DTOs;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.Cases.Queries.GetMyCases;

public class GetMyCasesQueryHandler : IRequestHandler<GetMyCasesQuery, Result<CursorPagedResult<CaseDto>>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetMyCasesQueryHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<CursorPagedResult<CaseDto>>> Handle(GetMyCasesQuery request, CancellationToken cancellationToken)
    {
        var pageSize = Math.Clamp(request.PageSize, 1, 50);

        var query = _db.Cases
            .Where(c => c.LawyerId == _currentUser.UserId);

        // ── Filters ──────────────────────────────────────────────────────
        if (request.Status.HasValue)
            query = query.Where(c => c.Status == request.Status.Value);

        if (request.Source.HasValue)
        {
            if (request.Source.Value == CaseSource.Platform)
                query = query.Where(c => c.CaseClientRequest != null);
            else
                query = query.Where(c => c.CaseClientRequest == null);
        }

        if (request.ServiceType.HasValue)
            query = query.Where(c => c.ServiceType == request.ServiceType.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(c => c.Title.Contains(request.Search));

        // ── Cursor pagination (ordered by Id descending) ─────────────────
        query = query.OrderByDescending(c => c.Id);

        if (request.Cursor.HasValue)
            query = query.Where(c => c.Id < request.Cursor.Value);

        var userId = _currentUser.UserId;

        var items = await query
            .Take(pageSize + 1) // fetch one extra to detect HasMore
            .Select(c => new CaseDto
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                ServiceType = c.ServiceType,
                Status = c.Status,
                CreatedAt = c.CreatedAt,
                Source = c.CaseClientRequest != null ? CaseSource.Platform : CaseSource.External,
                ClientName = c.ClientName ?? (c.CaseClientRequest != null ? c.CaseClientRequest.ClientRequest.Client.FullName : null),
                Reference = c.ReferenceNumber,
                LawyerId = c.LawyerId,
                ClientRequestId = c.CaseClientRequest != null ? c.CaseClientRequest.ClientRequestId : null,
                ClientRequestReference = c.CaseClientRequest != null ? c.CaseClientRequest.ClientRequest.RequestId : null,
                ServiceId = c.CaseClientRequest != null ? c.CaseClientRequest.ClientRequest.LawyerServiceLegalServiceId : null,
                Chat = c.Chat != null
                    ? c.Chat.ChatParticipants
                        .Where(cp => cp.UserId != userId)
                        .Select(cp => new CaseChatDto
                        {
                            ChatId = c.Chat.Id,
                            ReceiverId = cp.UserId,
                            FullName = cp.User.FullName,
                            ProfileImage = cp.User.AvatarUrl,
                            UnreadMessagesCount = c.Chat.ChatParticipants
                                .Where(me => me.UserId == userId)
                                .Select(me => me.UnReadMessageCount)
                                .FirstOrDefault()
                        })
                        .FirstOrDefault()
                    : null,
                CancellationReason = c.CancellationReason
            })
            .ToListAsync(cancellationToken);

        var hasMore = items.Count > pageSize;
        if (hasMore)
            items.RemoveAt(items.Count - 1);

        var result = new CursorPagedResult<CaseDto>
        {
            Items = items,
            HasMore = hasMore,
            NextCursor = hasMore && items.Count > 0 ? items[^1].Id : null
        };

        return Result<CursorPagedResult<CaseDto>>.Success(result);
    }
}
