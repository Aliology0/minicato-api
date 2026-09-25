using AlMostashar.Application.Common.Constants;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Common.Models;
using AlMostashar.Application.Features.Chat.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.Chat.Queries.GetChatMessages;

public class GetChatMessagesQueryHandler : IRequestHandler<GetChatMessagesQuery, Result<CursorPagedResult<ChatMessagesDto>>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetChatMessagesQueryHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<CursorPagedResult<ChatMessagesDto>>> Handle(GetChatMessagesQuery request, CancellationToken cancellationToken)
    {
        var pageSize = Math.Clamp(request.PageSize, 1, 50);

        // ── Verify current user is a participant ─────────────────────────
        var isParticipant = await _db.ChatParticipants
            .AnyAsync(cp => cp.ChatId == request.ChatId && cp.UserId == _currentUser.UserId, cancellationToken);

        if (!isParticipant)
            return Result<CursorPagedResult<ChatMessagesDto>>.Failure(
                new Error("Chat.Forbidden", Messages.Generic.NotFound("Chat")));

        // ── Query messages ───────────────────────────────────────────────
        var query = _db.ChatMessages
            .Where(m => m.ChatId == request.ChatId)
            .OrderByDescending(m => m.Id);

        if (request.NextCursor.HasValue)
            query = (IOrderedQueryable<Domain.Entities.ChatMessage>)
                query.Where(m => m.Id < request.NextCursor.Value);

        // ── Project & paginate ───────────────────────────────────────────
        var items = await query
            .Take(pageSize + 1)
            .Select(m => new ChatMessagesDto(
                m.Id,
                m.SenderId,
                m.MessageType,
                m.Content,
                m.IsRead,
                m.SentAt,
                m.ReadAt ?? DateTime.MinValue,
                m.CaseDocument!.Id 
            ))
            .ToListAsync(cancellationToken);

        var hasMore = items.Count > pageSize;
        int? nextCursor = null;
        if (hasMore)
        {
            items.RemoveAt(items.Count - 1);
            nextCursor = items[^1].MessageId;
        }

        var result = new CursorPagedResult<ChatMessagesDto>
        {
            Items = items,
            HasMore = hasMore,
            NextCursor = nextCursor
        };

        return Result<CursorPagedResult<ChatMessagesDto>>.Success(result);
    }
}
