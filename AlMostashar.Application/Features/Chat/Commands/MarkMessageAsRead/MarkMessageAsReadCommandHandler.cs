using AlMostashar.Application.Common.Constants;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.Chat.Commands.MarkMessageAsRead
{
    public class MarkMessageAsReadCommandHandler : IRequestHandler<MarkMessageAsReadCommand, Result<string>>
    {
        private readonly IAppDbContext _appDbContext;
        private readonly ICurrentUserService _currentUser;
        private readonly INotificationService _notificationService;

        public MarkMessageAsReadCommandHandler(IAppDbContext appDbContext, ICurrentUserService currentUser, INotificationService notificationService)
        {
            _appDbContext = appDbContext;
            _currentUser = currentUser;
            _notificationService = notificationService;
        }

        public async Task<Result<string>> Handle(MarkMessageAsReadCommand request, CancellationToken cancellationToken)
        {
            int currentUserId = _currentUser.UserId;

            var participant = await _appDbContext.ChatParticipants
                .FirstOrDefaultAsync(p => p.ChatId == request.ChatId && p.UserId == currentUserId, cancellationToken);

            if (participant is null)
                return Result<string>.Failure(new Error("Auth.UnauthorizedAccess", Messages.Generic.InvalidId("ChatId")));

            int lastMessageId = request.LastMessageId ?? 0;
            var rowsAffected = await _appDbContext.MarkMessagesAsReadAndResetCountAsync(request.ChatId, lastMessageId, currentUserId, cancellationToken);
            var updatedRowsCount = rowsAffected > 0 ? rowsAffected - 1 : 0; // subtract 1 since the Participant row update is also counted

            // Notify the other participant in real-time that their messages have been read
            if (updatedRowsCount > 0)
            {
                var otherParticipantUserId = await _appDbContext.ChatParticipants
                    .Where(p => p.ChatId == request.ChatId && p.UserId != currentUserId)
                    .Select(p => p.UserId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (otherParticipantUserId > 0)
                {
                    await _notificationService.SendMessagesReadAsync(
                        request.ChatId, currentUserId, otherParticipantUserId,
                        lastMessageId, DateTime.UtcNow, cancellationToken);
                }
            }

            return Result<string>.Success($"{updatedRowsCount} Messages updated successfully");
        }
    }
}
