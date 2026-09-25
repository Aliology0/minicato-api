using AlMostashar.Application.Common.Constants;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.Chat.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.Chat.Queries.GetChats
{
    public class GetChatsQueryHandler : IRequestHandler<GetChatsQuery, Result<ChatResponse>>
    {
        IAppDbContext _appDbContext;
        ICurrentUserService _currentUserService;

        public GetChatsQueryHandler(IAppDbContext appDbContext, ICurrentUserService currentUserService)
        {
            _appDbContext = appDbContext;
            _currentUserService = currentUserService;
        }

        public async Task<Result<ChatResponse>> Handle(GetChatsQuery request, CancellationToken cancellationToken)
        {
            int currentUserId = _currentUserService.UserId;
            var pageSize = Math.Clamp(request.PageSize, 1, 50);

            var query = _appDbContext.Chats.AsNoTracking().Where(chat => chat.ChatParticipants.Any(participant => participant.UserId == currentUserId));

            if (request.CursorDate.HasValue && request.NextCursor.HasValue)
            {
                query = query.Where(chat =>
                    chat.LastMessageAt < request.CursorDate.Value ||
                    (chat.LastMessageAt == request.CursorDate.Value && chat.Id < request.NextCursor.Value)
                );
            }

            query = query.OrderByDescending(chat => chat.LastMessageAt)
                         .ThenByDescending(chat => chat.Id);

            if (!string.IsNullOrEmpty(request.Search))
            {
                query = query.Where(chat => chat.ChatParticipants.Any(participant => participant.UserId != currentUserId && participant.User.FullName.Contains(request.Search)));
                // We will support search by message content later when we support a full-text search engine
            }

            var dbResult = await query.Select(chat => new
            {
                chat.Id,
                OtherParticipant = chat.ChatParticipants.Where(participant => participant.UserId != currentUserId).Select(participant => participant.User).FirstOrDefault(),
                chat.Case.ServiceType,
                LastMessageSenderId= chat.LastMessageSenderId,
                LastMessages = chat.LastMessageContent,
                LastMessageDate = chat.LastMessageAt,
                LastMessageType= chat.LastMessageType,
                MessageCount = chat.ChatParticipants.Where(participant => participant.UserId == currentUserId).Select(participant => participant.UnReadMessageCount).FirstOrDefault()
            })
            .Take(pageSize + 1)
            .ToListAsync(cancellationToken);

            var result = dbResult.Select(item => new ChatLookupDto(
                item.Id,
                item.OtherParticipant != null ? item.OtherParticipant.Id : 0,
                item.OtherParticipant != null ? item.OtherParticipant.FullName : string.Empty,
                item.OtherParticipant != null ? item.OtherParticipant.AvatarUrl : string.Empty,
                Messages.Enums.GetServiceType(item.ServiceType),
                item.LastMessages ?? string.Empty,
                item.LastMessageType,
                item.LastMessageDate,
                item.LastMessageSenderId == currentUserId,
                item.MessageCount
            )).ToList();

            int? nextCursorId = null;
            DateTime? nextCursorDate = null;
            bool hasMore = result.Count > pageSize;

            if (hasMore)
            {
                result.RemoveAt(result.Count - 1);
                var lastItem = result.Last();
                nextCursorId = lastItem.ChatId; 
                nextCursorDate = lastItem.LastMessageDate;
            }

            var pagedResult = new ChatResponse { HasMore = hasMore, Items = result, NextCursor = nextCursorId, CursorDate = nextCursorDate };
            return Result<ChatResponse>.Success(pagedResult);
        }
    }
}
