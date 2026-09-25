using AlMostashar.Application.Common.Constants;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.Events;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AlMostashar.Application.Features.Chat.EventHandlers;

public class ChatMessageCreatedEventHandler : INotificationHandler<ChatMessageCreatedEvent>
{
    private readonly IAppDbContext _db;
    private readonly INotificationService _notificationService;
    private readonly ILogger<ChatMessageCreatedEventHandler> _logger;

    public ChatMessageCreatedEventHandler(
        IAppDbContext db,
        INotificationService notificationService,
        ILogger<ChatMessageCreatedEventHandler> logger)
    {
        _db = db;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task Handle(ChatMessageCreatedEvent notificationEvent, CancellationToken cancellationToken)
    {
        try
        {
            var sender = await _db.Users
                .Where(u => u.Id == notificationEvent.SenderId)
                .Select(u => new { u.FullName, u.AvatarUrl })
                .FirstOrDefaultAsync(cancellationToken);

            var title = Messages.Notifications.NewMessageTitle;
            var body = Messages.Notifications.NewMessageBody(sender?.FullName??"");

            var notificationEntity = new Notification
            {
                UserId = notificationEvent.ReceiverId,
                Title = title,
                Description = body,
                Type = NotificationType.NewMessage,
                SourceId = notificationEvent.ChatId,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };
            
            await _db.Notifications.AddAsync(notificationEntity, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            // Firebase (FCM) integration should be placed here in the future
            await _notificationService.SendToUserAsync(
                notificationEvent.ReceiverId,
                title,
                body,
                NotificationType.NewMessage,
                notificationEvent.ChatId,
                sender?.FullName,
                sender?.AvatarUrl,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process ChatMessageCreatedEvent and send notifications for chat {ChatId}.", notificationEvent.ChatId);
        }
    }
}
